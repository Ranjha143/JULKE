using Dapper;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace JULKE
{
    internal class RetailPro_Shopify_SyncService : IJob
    {
        readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public RetailPro_Shopify_SyncService()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;

        }
        public async Task Execute(IJobExecutionContext context)
        {
            if (!AppVariables.Data_SyncService_InProgress)
            {
                await Task.Delay(0);
                AppVariables.Data_SyncService_InProgress = true;
                threadWorker.RunWorkerAsync();
            }
        }
        private void ThreadWorker_DoWork(object sender, DoWorkEventArgs e)
        {

#if DEBUG
            var postSaleOrderToServer = Task.Factory.StartNew(() => PostSaleOrderToServer().Wait());
            Task.WaitAll(postSaleOrderToServer);

            var postOrderCancellation = Task.Factory.StartNew(() => OrderCancleService().Wait());
            Task.WaitAll(postOrderCancellation);

#endif

#if !DEBUG

            var postSaleOrderToServer = Task.Factory.StartNew(() => PostSaleOrderToServer().Wait());
            Task.WaitAll(postSaleOrderToServer);

            var postOrderCancellation = Task.Factory.StartNew(() => OrderCancleService().Wait());
            Task.WaitAll(postOrderCancellation);
#endif
        }
        private void ThreadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        private void ThreadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AppVariables.Data_SyncService_InProgress = false;
        }


        private async Task<bool> PostSaleOrderToServer()
        {
            try
            {
                string connectionString = AppVariables.MongoConnectionString;
                MongoClient dbClient = new MongoClient(connectionString);
                var database = dbClient.GetDatabase(AppVariables.MongoDatabase);
                var orderCollection = database.GetCollection<BsonDocument>("Orders");

                IDbConnection connection = new OracleConnection(AppVariables.ConnectionString);
                var filterQuery = "{'isPosted':false, 'hasError':false}";
               // var filterQuery = "{'name':'100086016'}";

                var orderResult = (await orderCollection.FindAsync(filterQuery)).ToList();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var OrderList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ShopifyOrderModel>>(Newtonsoft.Json.JsonConvert.SerializeObject(orderObj));


                foreach (var order in OrderList)
                {
                    var orderPosted = await CreateSaleOrder(order);
                    if (!orderPosted.Equals("Error"))
                    {
                        var query = $@"
                            INSERT INTO web_orders
                                (so_sid,
                                 shopify_order_id,
                                 is_paid,
                                 shopify_created_date,
                                 coupon_code,
                                 disc_amt)
                            SELECT
                                TO_CHAR(d.sid) AS so_sid,
                                d.udf1_string AS shopify_order_id,
                                (case when so_deposit_amt_paid >0 then 1 else 0 end) AS is_paid,
                                to_char(TRUNC(created_datetime)) AS shopify_created_date,
                                ORDER_DISCOUNT_REASON_NAME AS coupon_code,
                                order_disc_amt AS disc_amt
                            FROM rps.document d
                            WHERE 1 = 1
                              AND d.order_doc_no IS NOT NULL
                              AND d.status > 3
                              AND d.is_held <> 1
                              AND NOT EXISTS (
                                    SELECT 1
                                    FROM web_orders w
                                    WHERE w.so_sid = d.sid)
                            ";

                        try
                        {
                            var res = await connection.ExecuteAsync(query);
                        }
                        catch (OracleException ex)
                        {
                            // ORA-xxxxx here
                        }
                        catch (Exception ex)
                        {
                            // other errors
                        }



                        var setPostFlagQuery = "{name:\"" + order.Name + "\"}";
                        var orderUpdate = Builders<BsonDocument>.Update
                            .Set("isPosted", true)
                            .Unset("error_message")
                            .Set("retailproSid",orderPosted);
                        await orderCollection.UpdateOneAsync(setPostFlagQuery, orderUpdate);
                    }
                }
            }
            catch (Exception)
            {
                // note_attributes
            }

            return true;
        }

        internal static async Task<string> CreateSaleOrder(ShopifyOrderModel OrderInfo)
        {
            BsonDocument logDocument = new BsonDocument();
            IDbConnection connection = new OracleConnection(AppVariables.ConnectionString);
            string connectionString = AppVariables.MongoConnectionString;
            MongoClient dbClient = new MongoClient(connectionString);
            var database = dbClient.GetDatabase(AppVariables.MongoDatabase);
            var orderCollection = database.GetCollection<BsonDocument>("Orders");
            var logCollection = database.GetCollection<BsonDocument>("logs");
            string postedDocumentSid = "";
            
            try
            {
                //var filterQuery = $@"{{name:'{OrderId}', isPosted: false }}";
                //var filterQuery = $@"{{name:'{OrderId}'}}";
                //var orderResult = await orderCollection.Find(filterQuery).ToListAsync();

                if (OrderInfo !=null)
                {
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory +"\\"+ OrderInfo.Name + ".txt", $"Order Processing Started for Order Id: {OrderInfo.Name} at {DateTime.Now}\n" + Environment.NewLine);

                    //var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    //var OrderInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ShopifyOrderModel>>(Newtonsoft.Json.JsonConvert.SerializeObject(orderObj)).FirstOrDefault();

                    var storeInfoQuery = $"Select SID,STORE_NAME, STORE_CODE, STORE_NO, ACTIVE_PRICE_LVL_SID, SBS_SID from Rps.Store where store_no = {ConfigurationManager.AppSettings["StoreNo"].ToString()}";
                    var storeInfo = connection.Query<StoreInfo>(storeInfoQuery).FirstOrDefault();

                    var OrderValidaterQuery = $"Select To_Char(SID) from Rps.Document where NOTES_GENERAL = '{OrderInfo.Name}'";

                    var ExistingOrder = connection.Query<string>(OrderValidaterQuery).FirstOrDefault();

                    if (ExistingOrder == null)
                    {
                        if (OrderInfo.PaymentGatewayNames == null || OrderInfo.PaymentGatewayNames.Count() == 0)
                        {

                            if (OrderInfo.TotalOutstanding > 0)
                            {
                                OrderInfo.PaymentGatewayNames.Add("Cash on Delivery (COD)");
                            }
                            else if (OrderInfo.TotalOutstanding == 0)
                            {
                                OrderInfo.PaymentGatewayNames.Add("Credit Card");
                            }
                            else
                            {
                                // skip order and log
                                logDocument = new BsonDocument { { "event", "Document POST" }, { "document_id", OrderInfo.Name }, { "message", "Payment Gateway is null" }, { "event_time_date", DateTime.Now } };
                                logCollection.InsertOne(logDocument);

                                var setPostFlagQuery = "{name:\"" + OrderInfo.Name + "\"}";
                                var orderUpdate = Builders<BsonDocument>.Update
                                    .Set("hasError", true)
                                    .Set("error_message", "No Payment Gateways found");
                                await orderCollection.UpdateOneAsync(setPostFlagQuery, orderUpdate);
                                return "Error";
                            }
                        }

                        if ((OrderInfo.Note !=null && OrderInfo.Note.ToLower().Contains("gift")) && (OrderInfo.PaymentGatewayNames.Any(g => g.ToLower().Contains("cod")) || OrderInfo.PaymentGatewayNames.Any(g => g.ToLower().Contains("cash on devlivery")) || OrderInfo.PaymentGatewayNames.Any(g => g.ToLower().Contains("gift"))) && OrderInfo.TotalOutstanding == 0 )
                        {
                            // condition for Free Gif Order Marked as PAID
                            OrderInfo.PaymentGatewayNames.RemoveAll(_=>true);
                            OrderInfo.PaymentGatewayNames.Add("Gift Card");
                        }

                        PostResponceObject customePostResponceInfo = new PostResponceObject();
                        PostResponceObject documentPostResponceInfo = new PostResponceObject();
                        #region ===================================================================================================== Customer 

                        var CustomerInfo = OrderInfo.Customer;
                        var BillingAddressInfo = OrderInfo.BillingAddress;
                        var ShippingAddressInfo = OrderInfo.ShippingAddress;

                        if (BillingAddressInfo == null) BillingAddressInfo = ShippingAddressInfo;

                        string CustomerPhoneNumber = CustomerInfo.Phone != null ? CustomerInfo.Phone : BillingAddressInfo.Phone != null ? BillingAddressInfo.Phone : ShippingAddressInfo.Phone != null ? ShippingAddressInfo.Phone : null;
                        string Customefirstrname = CustomerInfo.FirstName != null ? CustomerInfo.FirstName : BillingAddressInfo.FirstName != null ? BillingAddressInfo.FirstName : ShippingAddressInfo.FirstName != null ? ShippingAddressInfo.FirstName : null;
                        // string Customelastrname = CustomerInfo.LastName != null ? CustomerInfo.LastName : BillingAddressInfo.LastName != null ? BillingAddressInfo.LastName : ShippingAddressInfo.LastName != null ? ShippingAddressInfo.LastName : null;
                        //string addressline = BillingAddressInfo.Address1 != 
                        if (!string.IsNullOrEmpty(Customefirstrname))
                        {


                            if (!string.IsNullOrEmpty(CustomerPhoneNumber))
                            {
                                var CustomerQuery = " select C.SID, " +
                                    " C.CUST_ID, " +
                                    " C.FIRST_NAME, " +
                                    " C.LAST_NAME, " +
                                    " P.PHONE_NO " +
                                    " FROM Rps.customer C, " +
                                    " RPS.Customer_Phone P " +
                                    " WHERE C.SID = P.CUST_SID " +
                                    " AND substr(P.PHONE_NO,-10) like substr('%" + CustomerPhoneNumber + "',-10)";

                                var ExistingCustomer = connection.Query<Customer_Address_info>(CustomerQuery).FirstOrDefault();

                                int AddressCount = 0;
                                string BtCountry;

                                if (ExistingCustomer != null)
                                {
                                    customePostResponceInfo.Sid = ExistingCustomer.SID;
                                    BtCountry = BillingAddressInfo.Country;

                                    var completeAddress = BillingAddressInfo.Address1 + " " + BillingAddressInfo.Address2;

                                    var CustSid = ExistingCustomer.SID.ToString();
                                    var addressCountQuery = " select Count(*) from RPS.Customer_Address A where A.CUST_SID = " + CustSid + "";
                                    AddressCount = connection.Query<int>(addressCountQuery).FirstOrDefault();

                                    var existingAddressQuery = $" select PRIMARY_FLAG, ACTIVE, ADDRESS_1, ADDRESS_2, ADDRESS_3, CITY  from RPS.Customer_Address A where CUST_SID = {ExistingCustomer.SID} AND CITY like '{BillingAddressInfo.City}'";
                                    var existingAddress = connection.Query<Customer_Address_info>(existingAddressQuery).FirstOrDefault();

                                    if (existingAddress == null)
                                    {
                                        var address_line_1 = completeAddress.Length > 40 ? completeAddress.Substring(0, 40) : completeAddress;
                                        var address_line_2 = completeAddress.Length > 80 ? completeAddress.Substring(40, 40) : completeAddress.Length > 40 ? completeAddress.Substring(40, completeAddress.Length - 40) : "";
                                        var address_line_3 = (completeAddress.Length > 120) ? completeAddress.Substring(80, 40) : "";

                                        var PostAddress = new List<BillingAddress>();

                                        PostAddress.Add(new BillingAddress
                                        {
                                            origin_application = "OMNI",
                                            customer_sid = CustSid,
                                            active = true,
                                            address_allow_contact = false,
                                            primary_flag = true,
                                            City = BillingAddressInfo.City,
                                            address_line_1 = address_line_1,
                                            address_line_2 = address_line_2,
                                            address_line_3 = address_line_3,
                                            seq_no = ++AddressCount
                                        });

                                        string customerAddressPostLink = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/customer/" + CustSid + "/address";
                                        var customerPostResponse = RetailProApiCall.Post(customerAddressPostLink, PostAddress);
                                    }
                                    else
                                    {
                                        var existingAddressLine = existingAddress.ADDRESS_1 + existingAddress.ADDRESS_2 + existingAddress.ADDRESS_3;

                                        completeAddress = RemoveSpecialCharacters(completeAddress);
                                        existingAddressLine = RemoveSpecialCharacters(existingAddressLine);

                                        var aString1 = completeAddress.ToUpper().Replace(",", " ").Split(' ').ToList().OrderBy(s => s);
                                        var aString2 = existingAddressLine.ToUpper().Replace(",", " ").Split(' ').ToList().OrderBy(s => s);

                                        var sameAddress = Enumerable.SequenceEqual(aString1, aString2);

                                        if (!sameAddress)
                                        {
                                            var address_line_1 = completeAddress.Length > 40 ? completeAddress.Substring(0, 40) : completeAddress;
                                            var address_line_2 = completeAddress.Length > 80 ? completeAddress.Substring(40, 40) : completeAddress.Length > 40 ? completeAddress.Substring(40, completeAddress.Length - 40) : "";
                                            var address_line_3 = (completeAddress.Length > 120) ? completeAddress.Substring(80, 40) : "";

                                            var PostAddress = new List<BillingAddress>();

                                            PostAddress.Add(new BillingAddress
                                            {
                                                origin_application = "OMNI",
                                                customer_sid = CustSid,
                                                active = true,
                                                address_allow_contact = false,
                                                primary_flag = true,
                                                City = BillingAddressInfo.City,
                                                address_line_1 = address_line_1,
                                                address_line_2 = address_line_2,
                                                address_line_3 = address_line_3,
                                                seq_no = ++AddressCount
                                            });

                                            string customerAddressPostLink = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/customer/" + CustSid + "/address";
                                            var customerPostResponse = RetailProApiCall.Post(customerAddressPostLink, PostAddress);
                                        }
                                    }
                                }
                                else // Create Customer
                                {
                                    List<PostCustomer> postCustomer = new List<PostCustomer>();
                                    PostCustomer customer = new PostCustomer
                                    {
                                        origin_application = "OMNI",
                                        store_sid = storeInfo.SID,
                                        last_name = BillingAddressInfo.LastName,
                                        first_name = BillingAddressInfo.FirstName,
                                        customer_active = 1,
                                        customer_type = 0,
                                        full_name = (BillingAddressInfo.FirstName + " " + BillingAddressInfo.LastName).Trim(),
                                        phones = new List<Phone>(),
                                        address = new List<BillingAddress>()
                                    };

                                    customer.phones.Add(new Phone
                                    {
                                        origin_application = "OMNI",
                                        phone_no = CustomerInfo.Phone != null ? CustomerInfo.Phone : BillingAddressInfo.Phone,
                                        primary_flag = true,
                                        seq_no = 1,
                                    });

                                    var completeAddress = BillingAddressInfo.Address1 + " " + BillingAddressInfo.Address2;
                                    var address_line_1 = completeAddress.Length > 40 ? completeAddress.Substring(0, 40) : completeAddress;
                                    var address_line_2 = completeAddress.Length > 80 ? completeAddress.Substring(40, 40) : completeAddress.Length > 40 ? completeAddress.Substring(40, completeAddress.Length - 40) : "";
                                    var address_line_3 = (completeAddress.Length > 120) ? completeAddress.Substring(80, 40) : "";

                                    customer.address.Add(new BillingAddress
                                    {
                                        origin_application = "OMNI",
                                        active = true,
                                        address_allow_contact = false,
                                        primary_flag = true,
                                        City = BillingAddressInfo.City,
                                        address_line_1 = address_line_1,
                                        address_line_2 = address_line_2,
                                        address_line_3 = address_line_3,
                                        address_line_5 = BillingAddressInfo.City,
                                        seq_no = ++AddressCount
                                    });
                                    postCustomer.Add(customer);

                                    string customerPostLink = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/customer";
                                    var customerPostResponse = RetailProApiCall.Post(customerPostLink, postCustomer);

                                    if (customerPostResponse.StatusCode == HttpStatusCode.Created)
                                    {
                                        var customePostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(customerPostResponse.Content);
                                        customePostResponceInfo = customePostResponceObject.FirstOrDefault();
                                    }
                                }

                        #endregion

                                #region ====================================================================================================== Line Items 

                                var lineItems = OrderInfo.LineItems;
                                var incomingSkuList = lineItems.Select(a => a.Sku).ToList();
                                var skuString = "'" + String.Join("','", incomingSkuList) + "'";
                                var itemCheckQuery = $"select SID, UPC,ALU, DESCRIPTION1, ACTIVE from RPS.invn_sbs_item where UPC in ({skuString}) and ACTIVE = 1";
                                var itemsResult = connection.Query<ItemInfo>(itemCheckQuery).ToList();

                                var existingSkuList = itemsResult.Select(s => s.UPC).ToList();

                                var exceptionItems = incomingSkuList.Except(existingSkuList).ToList();

                                if (exceptionItems.Count > 0)
                                {
                                    logDocument = new BsonDocument { { "event", "Document POST" }, { "document_id", OrderInfo.Name }, { "message", "Order Id Not found in POS" }, { "event_time_date", DateTime.Now } };
                                    return "Error";
                                }

                                var priceLvlQry = "select PRICE_LVL from rps.price_level  where sid = " + storeInfo.ACTIVE_PRICE_LVL_SID;
                                var activePriceLevelId = connection.Query<string>(priceLvlQry).FirstOrDefault();

                                var TOTAL_LINE_ITEM = lineItems.Count();
                                var ORDER_QTY = lineItems.Sum(s => s.Quantity);
                                List<ItemPostInfo> ItemPostInfo = new List<ItemPostInfo>();
                                int itemPos = 1;
                                foreach (var item in lineItems)
                                {

                                    var refItem = itemsResult.Where(a => a.UPC == item.Sku).FirstOrDefault();
                                    var priceQry =
                                        "select PRICE from rps.invn_sbs_price where PRICE_LVL_SID = '" +
                                        storeInfo.ACTIVE_PRICE_LVL_SID +
                                        "' AND INVN_SBS_ITEM_SID = '" + refItem.SID + "'";


                                    var itemPrice = connection.Query<Decimal>(priceQry).FirstOrDefault();

                                    ItemPostInfo.Add(new ItemPostInfo
                                    {
                                        ORIGIN_APPLICATION = "OMNI",
                                        INVN_SBS_ITEM_SID = refItem.SID,
                                        Order_Type = 0,
                                        Item_Type = 3,
                                        FULFILL_STORE_SID = storeInfo.SID,
                                        MANUAL_DISC_TYPE = 0,
                                        QUANTITY = item.Quantity,
                                        MANUAL_DISC_VALUE = string.IsNullOrEmpty(item.Price) ? 0 : Convert.ToDecimal(item.Price)
                                    });
                                    itemPos++;
                                }

                                var CustomerPoNumber = "COD";
                                if (OrderInfo.PaymentGatewayNames.Contains("Gift Card"))
                                {
                                    CustomerPoNumber = "Gift Card";
                                }
                                else if (!OrderInfo.PaymentGatewayNames.Any(g => g.ToUpper().Contains("COD")) && !OrderInfo.PaymentGatewayNames.Contains("Gift Card"))
                                {
                                    CustomerPoNumber = "Paid";
                                }
                                else if (OrderInfo.PaymentGatewayNames.Contains("Bank Deposit"))
                                {
                                    CustomerPoNumber = "Paid";
                                }

                                List<PostDocument> postDocument = new List<PostDocument>();

                                var CUSTOMER_PO_NUMBER = "COD";
                                if (CustomerPoNumber == "Paid")
                                {
                                    CUSTOMER_PO_NUMBER = "Card";
                                    if (OrderInfo.PaymentGatewayNames.FirstOrDefault().Contains("BaadMay") || OrderInfo.PaymentGatewayNames.FirstOrDefault().Contains("Pay Later"))
                                        CUSTOMER_PO_NUMBER = "BNPL";
                                    if (OrderInfo.PaymentGatewayNames.FirstOrDefault().Contains("bank_alfalah_mpgs_payment_gateway_all_master_visa_cards_are_accepted_"))
                                        CUSTOMER_PO_NUMBER = "BAF";
                                    if (OrderInfo.PaymentGatewayNames.Contains("Bank Deposit"))
                                        CUSTOMER_PO_NUMBER = "B Deposit";

                                }
                                else if (CustomerPoNumber == "Gift Card")
                                {
                                    CUSTOMER_PO_NUMBER = "Gift Card";
                                }

                                var completeAddresss = "";
                                var address_line_1s = "";
                                var address_line_2s = "";
                                var address_line_3s = "";
                                var bt_primary_no = "";
                                var St_completeAddresss = "";
                                var St_address_line_1s = "";
                                var St_address_line_2s = "";
                                var St_address_line_3s = "";
                                var st_primary_no = "";
                                var bt_firstname = "";
                                var st_firstname = "";
                                var bt_lastname = "";
                                var st_lastname = "";
                               

                                if (BillingAddressInfo != null && ShippingAddressInfo != null)
                                {
                                    completeAddresss = BillingAddressInfo.Address1 + " " + BillingAddressInfo.Address2;
                                    address_line_1s = completeAddresss.Length > 40 ? completeAddresss.Substring(0, 40) : completeAddresss;
                                    address_line_2s = completeAddresss.Length > 80 ? completeAddresss.Substring(40, 40) : completeAddresss.Length > 40 ? completeAddresss.Substring(40, completeAddresss.Length - 40) : "";
                                    address_line_3s = (completeAddresss.Length > 120) ? completeAddresss.Substring(80, 40) : "";
                                    bt_primary_no = BillingAddressInfo.Phone;
                                    bt_firstname = BillingAddressInfo.FirstName;
                                    bt_lastname = BillingAddressInfo.LastName;


                                    St_completeAddresss = ShippingAddressInfo.Address1 + " " + ShippingAddressInfo.Address2;
                                    St_address_line_1s = St_completeAddresss.Length > 40 ? St_completeAddresss.Substring(0, 40) : St_completeAddresss;
                                    St_address_line_2s = St_completeAddresss.Length > 80 ? St_completeAddresss.Substring(40, 40) : St_completeAddresss.Length > 40 ? St_completeAddresss.Substring(40, St_completeAddresss.Length - 40) : "";
                                    St_address_line_3s = (St_completeAddresss.Length > 120) ? St_completeAddresss.Substring(80, 40) : "";
                                    st_primary_no = ShippingAddressInfo.Phone;
                                    st_firstname = ShippingAddressInfo.FirstName;
                                    st_lastname = ShippingAddressInfo.LastName; 

                                }
                                else if (BillingAddressInfo != null && ShippingAddressInfo == null)
                                {
                                    completeAddresss = BillingAddressInfo.Address1 + " " + BillingAddressInfo.Address2;
                                    address_line_1s = completeAddresss.Length > 40 ? completeAddresss.Substring(0, 40) : completeAddresss;
                                    address_line_2s = completeAddresss.Length > 80 ? completeAddresss.Substring(40, 40) : completeAddresss.Length > 40 ? completeAddresss.Substring(40, completeAddresss.Length - 40) : "";
                                    address_line_3s = (completeAddresss.Length > 120) ? completeAddresss.Substring(80, 40) : "";
                                    bt_primary_no = BillingAddressInfo.Phone;
                                    bt_firstname = BillingAddressInfo.FirstName;
                                    bt_lastname = BillingAddressInfo.LastName;

                                    St_completeAddresss = BillingAddressInfo.Address1 + " " + BillingAddressInfo.Address2;
                                    St_address_line_1s = completeAddresss.Length > 40 ? completeAddresss.Substring(0, 40) : completeAddresss;
                                    St_address_line_2s = completeAddresss.Length > 80 ? completeAddresss.Substring(40, 40) : completeAddresss.Length > 40 ? completeAddresss.Substring(40, completeAddresss.Length - 40) : "";
                                    St_address_line_3s = (completeAddresss.Length > 120) ? completeAddresss.Substring(80, 40) : "";
                                    st_primary_no = BillingAddressInfo.Phone;
                                    st_firstname = BillingAddressInfo.FirstName;
                                    st_lastname = BillingAddressInfo.LastName;

                                }
                                else if (BillingAddressInfo == null && ShippingAddressInfo != null)
                                {

                                    completeAddresss = ShippingAddressInfo.Address1 + " " + ShippingAddressInfo.Address2;
                                    address_line_1s = St_completeAddresss.Length > 40 ? St_completeAddresss.Substring(0, 40) : St_completeAddresss;
                                    address_line_2s = St_completeAddresss.Length > 80 ? St_completeAddresss.Substring(40, 40) : St_completeAddresss.Length > 40 ? St_completeAddresss.Substring(40, St_completeAddresss.Length - 40) : "";
                                    address_line_3s = (St_completeAddresss.Length > 120) ? St_completeAddresss.Substring(80, 40) : "";
                                    bt_primary_no = ShippingAddressInfo.Phone;
                                    bt_firstname = ShippingAddressInfo.FirstName;
                                    bt_lastname = ShippingAddressInfo.LastName;


                                    St_completeAddresss = ShippingAddressInfo.Address1 + " " + ShippingAddressInfo.Address2;
                                    St_address_line_1s = St_completeAddresss.Length > 40 ? St_completeAddresss.Substring(0, 40) : St_completeAddresss;
                                    St_address_line_2s = St_completeAddresss.Length > 80 ? St_completeAddresss.Substring(40, 40) : St_completeAddresss.Length > 40 ? St_completeAddresss.Substring(40, St_completeAddresss.Length - 40) : "";
                                    St_address_line_3s = (St_completeAddresss.Length > 120) ? St_completeAddresss.Substring(80, 40) : "";
                                    st_primary_no = ShippingAddressInfo.Phone;
                                    st_firstname = ShippingAddressInfo.FirstName;
                                    st_lastname = ShippingAddressInfo.LastName;
                                }



                                PostDocument document = new PostDocument
                                {
                                    ORIGIN_APPLICATION = "OMNI",
                                    BT_CUID = customePostResponceInfo.Sid,
                                    BT_COUNTRY = BillingAddressInfo.Country,
                                    BT_ADDRESS_LINE1 = address_line_1s,
                                    BT_ADDRESS_LINE2 = address_line_2s,
                                    BT_ADDRESS_LINE3 = address_line_3s,
                                    BT_ADDRESS_LINE4 = "",
                                    BT_ADDRESS_LINE5 = BillingAddressInfo.City,
                                    BT_PRIMARY_PHONE_NO = BillingAddressInfo.Phone ?? CustomerInfo.Phone,
                                   BT_FIRST_NAME = bt_firstname,
                                   BT_LAST_NAME = bt_lastname,
                                    //ST_CUID = customePostResponceInfo.Sid,
                                    ST_COUNTRY = ShippingAddressInfo.Country,
                                    ST_PRIMARY_PHONE_NO = st_primary_no,
                                    ST_ADDRESS_LINE1 = St_address_line_1s,
                                    ST_ADDRESS_LINE2 = St_address_line_2s,
                                    ST_ADDRESS_LINE3 = St_address_line_3s,
                                    ST_ADDRESS_LINE4 = "",
                                    ST_ADDRESS_LINE5 = ShippingAddressInfo.City,
                                    ST_FIRST_NAME = st_firstname,
                                    ST_LAST_NAME = st_lastname,
                                    SUBSIDIARY_UID = storeInfo.SBS_SID,
                                    STORE_SID = storeInfo.SID,
                                    STORE_NO = storeInfo.STORE_NO,
                                    STORE_NAME = storeInfo.STORE_NAME,
                                    NOTES_GENERAL = OrderInfo.Name,
                                    CUSTOMER_PO_NUMBER = CUSTOMER_PO_NUMBER,
                                    UDF_STRING1 = OrderInfo.Id.ToString(),
                                    UDF_STRING2 = "NORM",
                                    UDF_STRING3 = "1",
                                    UDF_STRING4 = "",
                                    UDF_STRING5 = OrderInfo.OrderNumber.ToString(),
                                    ORDER_STATUS = 0,
                                    SEND_SALE_FULFILLMENT = false,
                                    POS_FLAG1 = "ECOM"
                                };

                                postDocument.Add(document);

                                var documentPostLink = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document";
                                var documentResponse = RetailProApiCall.Post(documentPostLink, postDocument);

                                if (documentResponse.StatusCode == HttpStatusCode.Created)
                                {
                                    var docPostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(documentResponse.Content);
                                    var docPostResponceInfo = docPostResponceObject.FirstOrDefault();

                                    string itemPostLink = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" +
                                            docPostResponceInfo.Sid + "/item";

                                    postedDocumentSid = docPostResponceInfo.Sid;

                                    var docItemPost = RetailProApiCall.Post(itemPostLink, ItemPostInfo);
                                    if (docItemPost.StatusCode == HttpStatusCode.Created)
                                    {
                                        if (OrderInfo.DiscountCodes.Count > 0)
                                        {
                                            List<PutOrderDiscount> orderDiscountObject = new List<PutOrderDiscount>();
                                            orderDiscountObject.Add(new PutOrderDiscount
                                            {
                                                MANUAL_ORDER_DISC_TYPE = "2",
                                                MANUAL_ORDER_DISC_VALUE = OrderInfo.DiscountCodes.FirstOrDefault().Amount,
                                                MANUAL_DISC_REASON = null,
                                                MANUAL_ORDER_DISC_REASON = OrderInfo.DiscountCodes.FirstOrDefault().Code
                                            });

                                            var docRowVersionForDiscount = await RetailProApiCall.GetAsync(ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + docPostResponceInfo.Sid + "?cols=*");
                                            docPostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(docRowVersionForDiscount.Content);
                                            docPostResponceInfo = docPostResponceObject.FirstOrDefault();

                                            var orderDiscountPutUrl = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + docPostResponceInfo.Sid + "?cols=*&filter=row_version,eq," + docPostResponceInfo.RowVersion;
                                            var documentDiscountResponce = RetailProApiCall.PUT(orderDiscountPutUrl, orderDiscountObject);

                                        }

                                        if (OrderInfo.ShippingLines.Count() > 0)
                                        {
                                            var shippingSidQuery = "SELECT TO_CHAR(SID) FROM RPS.SHIP_METHOD WHERE METHOD = 'Other'";
                                            var shippinfSid = connection.Query<string>(shippingSidQuery).FirstOrDefault();

                                            List<PutShippingInfo> putShippingInfo = new List<PutShippingInfo>();

                                            putShippingInfo.Add(new PutShippingInfo
                                            {
                                                ORDER_SHIP_METHOD_SID = shippinfSid,
                                                ORDER_SHIPPING_AMT_MANUAL = OrderInfo.ShippingLines.FirstOrDefault().Price
                                            });

                                            var docRowVersionForShipping = await RetailProApiCall.GetAsync(ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + docPostResponceInfo.Sid + "?cols=*");
                                            docPostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(docRowVersionForShipping.Content);
                                            docPostResponceInfo = docPostResponceObject.FirstOrDefault();

                                            var orderShippingPutUrl = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + docPostResponceInfo.Sid + "?cols=*&filter=row_version,eq," + docPostResponceInfo.RowVersion;
                                            var documentDiscountResponce = RetailProApiCall.PUT(orderShippingPutUrl, putShippingInfo);
                                        }

                                        if (OrderInfo.Gateway != null && OrderInfo.Gateway.Equals("2CO"))
                                        {
                                            List<PutTaxInfo> taxInfoObject = new List<PutTaxInfo>();
                                            taxInfoObject.Add(new PutTaxInfo { TAX_AREA_NAME = "EXEMPT" });

                                            var docRowVersionForShipping = await RetailProApiCall.GetAsync(ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + docPostResponceInfo.Sid + "?cols=*");
                                            docPostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(docRowVersionForShipping.Content);
                                            docPostResponceInfo = docPostResponceObject.FirstOrDefault();

                                            var orderTaxPutUrl = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + docPostResponceInfo.Sid + "?cols=*&filter=row_version,eq," + docPostResponceInfo.RowVersion;
                                            var documentDiscountResponce = RetailProApiCall.PUT(orderTaxPutUrl, taxInfoObject);
                                        }

                                        string docRowVersionLink1 = $"{ConfigurationManager.AppSettings["ServerWebAddress"]}/v1/rest/document/" + docPostResponceInfo.Sid + "?cols=*";
                                        var docRowVersionResponse1 = await RetailProApiCall.GetAsync(docRowVersionLink1);
                                        docPostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(docRowVersionResponse1.Content);
                                        docPostResponceInfo = docPostResponceObject.FirstOrDefault();

                                        var soDepositCallLink = $"{ConfigurationManager.AppSettings["ServerWebAddress"]}/v1/rest/document/" +
                                        docPostResponceInfo.Sid + "?filter=row_version,eq," + docPostResponceInfo.RowVersion;

                                        List<SODeposit> depositRequestList = new List<SODeposit>();

                                        decimal.TryParse(OrderInfo.TotalPrice.ToString(), out decimal totalPrice);
                                        decimal.TryParse(OrderInfo.TotalOutstanding.ToString(), out decimal totalOutstanding);

                                        decimal SoPaidamount = totalPrice - totalOutstanding;

                                        if (CustomerPoNumber == "COD" && totalPrice == totalOutstanding) //3=COD
                                        {
                                            SoPaidamount = 0;
                                        }


                                        if (OrderInfo.PaymentGatewayNames.Contains("Bank Deposit") && totalPrice == totalOutstanding)
                                        {
                                            SoPaidamount = totalOutstanding;
                                        }


                                        SODeposit depositRequest = new SODeposit
                                        {
                                            tenders = new List<object>(),
                                            so_deposit_amt_paid = SoPaidamount.ToString()
                                        };

                                        depositRequestList.Add(depositRequest);

                                        var depositResponse = RetailProApiCall.PUT(soDepositCallLink, depositRequestList);

                                        if (depositResponse.IsSuccessful)
                                        {

                                            // TENDER
                                            List<PostTender> tenderPayload = new List<PostTender>();
                                            if (CustomerPoNumber.ToUpper() == "COD") //3=COD
                                            {
                                                tenderPayload.Add(new PostTender
                                                {
                                                    ORIGIN_APPLICATION = "OMNI",
                                                    DOCUMENT_SID = docPostResponceInfo.Sid,
                                                    TAKEN = SoPaidamount.ToString(),
                                                    TENDER_TYPE = "3",
                                                    TENDER_NAME = "COD"

                                                });

                                            }
                                            else if (CustomerPoNumber.ToUpper() == "GIFT CARD") //10=GiftCard
                                            {
                                                tenderPayload.Add(new PostTender
                                                {
                                                    ORIGIN_APPLICATION = "OMNI",
                                                    DOCUMENT_SID = docPostResponceInfo.Sid,
                                                    TAKEN = SoPaidamount.ToString(),
                                                    TENDER_TYPE = "10",
                                                    TENDER_NAME = "Gift Card"
                                                });

                                            }
                                            else if (OrderInfo.PaymentGatewayNames.Contains("Bank Deposit")) //10=GiftCard
                                            {
                                                tenderPayload.Add(new PostTender
                                                {
                                                    ORIGIN_APPLICATION = "OMNI",
                                                    DOCUMENT_SID = docPostResponceInfo.Sid,
                                                    TAKEN = SoPaidamount.ToString(),
                                                    TENDER_TYPE = "2",
                                                    TENDER_NAME = "B Deposit"
                                                });

                                            }
                                            else if (CustomerPoNumber.ToUpper() == "PAID") //2=CreditCard
                                            {
                                                var TenderName = "Credit Card";

                                                if (OrderInfo.PaymentGatewayNames.FirstOrDefault().Contains("BaadMay") || OrderInfo.PaymentGatewayNames.FirstOrDefault().Contains("Pay Later"))
                                                    TenderName = "BNPL";

                                                if (OrderInfo.PaymentGatewayNames.FirstOrDefault().Contains("bank_alfalah_mpgs_payment_gateway_all_master_visa_cards_are_accepted_"))
                                                    TenderName = "BAF";

                                                tenderPayload.Add(new PostTender
                                                {
                                                    ORIGIN_APPLICATION = "OMNI",
                                                    DOCUMENT_SID = docPostResponceInfo.Sid,
                                                    TAKEN = SoPaidamount.ToString(),
                                                    TENDER_TYPE = "2",
                                                    TENDER_NAME = TenderName
                                                });

                                            }


                                            if (SoPaidamount > 0)
                                            {
                                                string docTenderLink = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + docPostResponceInfo.Sid + "/tender";
                                                var tenderResponse = RetailProApiCall.Post(docTenderLink, tenderPayload);
                                            }

                                            string docRowVersionLink =
                                                    ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + docPostResponceInfo.Sid + "?cols=*";
                                            var docRowVersionResponse = await RetailProApiCall.GetAsync(docRowVersionLink);
                                            docPostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(docRowVersionResponse.Content);
                                            docPostResponceInfo = docPostResponceObject.FirstOrDefault();

                                            string documentStatusLink = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" +
                                                docPostResponceInfo.Sid + "?filter=ROW_VERSION,eq," + docPostResponceInfo.RowVersion;

                                            List<OrderStatusPut> orderStatusPuts = new List<OrderStatusPut>
                                        {
                                            new OrderStatusPut
                                            {
                                                //ORDER_TYPE = 0,
                                                STATUS = 4
                                            }
                                        };

                                            var documentStatusResponce = RetailProApiCall.PUT(documentStatusLink, orderStatusPuts);
                                            docPostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(documentStatusResponce.Content);
                                            docPostResponceInfo = docPostResponceObject.FirstOrDefault();

                                            logDocument = new BsonDocument { { "event", "Document Posted" }, { "document_id", OrderInfo.Name }, { "message", "Document Posted Sucessfully" }, { "event_time_date", DateTime.Now } };
                                            logCollection.InsertOne(logDocument);
                                            return postedDocumentSid;
                                        }
                                        else
                                        {
                                            var filter = Builders<BsonDocument>.Filter.Eq("document_id", OrderInfo.Name);
                                            var options = new ReplaceOptions { IsUpsert = true };
                                            logDocument = new BsonDocument { { "event", "Document Deport Post" }, { "document_id", OrderInfo.Name }, { "message", "Document Items failed to post in RetailPro " + depositResponse.Content }, { "event_time_date", DateTime.Now } };
                                            logCollection.ReplaceOne(filter, logDocument, options);

                                            return "Error";
                                        }
                                    }
                                    else
                                    {
                                        var filter = Builders<BsonDocument>.Filter.Eq("document_id", OrderInfo.Name);
                                        var options = new ReplaceOptions { IsUpsert = true };
                                        logDocument = new BsonDocument { { "event", "Document Item Post" }, { "document_id", OrderInfo.Name }, { "message", "Document Items failed to post in RetailPro " + docItemPost.Content }, { "event_time_date", DateTime.Now } };
                                        logCollection.ReplaceOne(filter, logDocument, options);

                                        return "Error";
                                    }
                                }
                                else
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("document_id", OrderInfo.Name);
                                    var options = new ReplaceOptions { IsUpsert = true };
                                    logDocument = new BsonDocument { { "event", "Document Post" }, { "document_id", OrderInfo.Name }, { "message", "Document failed to post in RetailPro " + documentResponse.Content }, { "event_time_date", DateTime.Now } };
                                    logCollection.ReplaceOne(filter, logDocument, options);

                                    return "Error";
                                }

                                #endregion
                            }
                            else
                            {
                                var filter = Builders<BsonDocument>.Filter.Eq("document_id", OrderInfo.Name);
                                var options = new ReplaceOptions { IsUpsert = true };
                                logDocument = new BsonDocument { { "event", "Customer Search" }, { "document_id", OrderInfo.Name }, { "message", "Customer Phone Number not found in original Order" }, { "event_time_date", DateTime.Now } };
                                logCollection.ReplaceOne(filter, logDocument, options);

                                return "Error";
                            }
                        }
                        else
                        {
                            var filter = Builders<BsonDocument>.Filter.Eq("document_id", OrderInfo.Name);
                            var options = new ReplaceOptions { IsUpsert = true };
                            logDocument = new BsonDocument { { "event", "Customer Search" }, { "document_id", OrderInfo.Name }, { "message", "Customer Name not found in original Order" }, { "event_time_date", DateTime.Now } };
                            logCollection.ReplaceOne(filter, logDocument, options);

                            return "Error";
                        }
                    }
                    else
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("document_id", OrderInfo.Name);
                        var options = new ReplaceOptions { IsUpsert = true };
                        logDocument = new BsonDocument { { "event", "Order Processing" }, { "document_id", OrderInfo.Name }, { "message", "Duplicate Order Id found in original Order" }, { "event_time_date", DateTime.Now } };
                        logCollection.ReplaceOne(filter, logDocument, options);

                        var setPostFlagQuery = "{name:\"" + OrderInfo.Name + "\"}";
                        var orderUpdate = Builders<BsonDocument>.Update
                            .Set("isPosted", true)
                            .Set("retailproSid", ExistingOrder);
                        await orderCollection.UpdateOneAsync(setPostFlagQuery, orderUpdate);

                        return ExistingOrder;
                    }
                }
                else
                {
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                
                logDocument = new BsonDocument { { "event", "Order Processing" }, { "document_id", "" }, { "message", "Exception message: " + ex.Message + ex.InnerException?.Message }, { "event_time_date", DateTime.Now } };
                logCollection.InsertOne(logDocument);

                return "Error";
            }
        }


        private async Task<bool> OrderCancleService()
        {

            try
            {
                string connectionString = AppVariables.MongoConnectionString;
                MongoClient dbClient = new MongoClient(connectionString);
                var database = dbClient.GetDatabase(AppVariables.MongoDatabase);
                var orderCollection = database.GetCollection<BsonDocument>("OrdersCanceled");
                //"invoiced", 1
                IDbConnection connection = new OracleConnection(AppVariables.ConnectionString);
                var filterQuery = "{'isPosted':false, 'hasError':false}";
                //var filterQuery = "{'isPosted':false}";

                var orderResult = (await orderCollection.FindAsync(filterQuery)).ToList();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var OrderList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ShopifyOrderModel>>(Newtonsoft.Json.JsonConvert.SerializeObject(orderObj));

                foreach (var order in OrderList)
                {
                    if (order.CancelledAt != null)
                    {

                        var checkForOrderPostedQuery = $"Select  To_Char(sid) as SID, SO_CANCEL_FLAG, ORIGIN_APPLICATION from Rps.Document where NOTES_GENERAL = '{order.Name}'";
                        var ExistingCancelledOrderInfo = connection.Query<ExistingCancelledOrders>(checkForOrderPostedQuery).FirstOrDefault();

                        if (ExistingCancelledOrderInfo != null)
                        {
                            var orderIsCancelled = ExistingCancelledOrderInfo.SO_CANCEL_FLAG == "1";

                            if (!orderIsCancelled)
                            {
                                var docRowVersionLink1 = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" + ExistingCancelledOrderInfo.SID + "?cols=*";
                                var docRowVersionResponse1 = await RetailProApiCall.GetAsync(docRowVersionLink1);
                                var docPostResponceObject = JsonConvert.DeserializeObject<List<PostResponceObject>>(docRowVersionResponse1.Content);
                                var docPostResponceInfo = docPostResponceObject.FirstOrDefault();

                                List<PutDocumentCancle> postDocument_put = new List<PutDocumentCancle>();
                                PutDocumentCancle document_put = new PutDocumentCancle
                                {
                                    ORIGIN_APPLICATION = ExistingCancelledOrderInfo.ORIGIN_APPLICATION,
                                    SO_CANCEL_FLAG = true
                                    //NOTES_GENERAL = "-"
                                };
                                postDocument_put.Add(document_put);
                                var documentStatusLink = ConfigurationManager.AppSettings["ServerWebAddress"] + "/v1/rest/document/" +
                                    ExistingCancelledOrderInfo.SID + "?filter=ROW_VERSION,eq," + docPostResponceInfo.RowVersion;
                                var documentStatusResponce = RetailProApiCall.PUT(documentStatusLink, postDocument_put);

                                var updateFilter = Builders<BsonDocument>.Filter.Eq("name", order.Name);
                                var update = Builders<BsonDocument>.Update.Set("isPosted", true);
                                await orderCollection.UpdateOneAsync(updateFilter, update);

                            }
                            else
                            {
                                // update mongodB order set flag isPosted = true where name = order.Name
                                var updateFilter = Builders<BsonDocument>.Filter.Eq("name", order.Name);
                                var update = Builders<BsonDocument>.Update.Set("isPosted", true);
                                await orderCollection.UpdateOneAsync(updateFilter, update);
                            }
                        }
                        else
                        {
                            // update mongodB order set flag isPosted = true where name = order.Name
                            var updateFilter = Builders<BsonDocument>.Filter.Eq("name", order.Name);
                            var update = Builders<BsonDocument>.Update.Set("hasError", true).Set("error_message","Order Not Found");
                            await orderCollection.UpdateOneAsync(updateFilter, update);
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
    }

    public class PutDocumentCancle
    {
        public string ORIGIN_APPLICATION { get; set; }
        //public string NOTES_GENERAL { get; set; }
        public bool SO_CANCEL_FLAG { get; set; } = false;
    }

    public class ExistingCancelledOrders
    {

        public string SID { get; set; }
        public string SO_CANCEL_FLAG { get; set; }
        public string ORIGIN_APPLICATION { get; set; }
    }


}