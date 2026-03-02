using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JULKE
{
    public class RetailProModels
    {
    }

    public class StoreInfo
    {
        public string SID { get; set; }
        public string STORE_NAME { get; set; }
        public string SBS_SID { get; set; }
        public string STORE_CODE { get; set; }
        public string STORE_NO { get; set; }
        public string ACTIVE_PRICE_LVL_SID { get; set; }
    }

    public class Customer_Address_info
    {
        public string SID { get; set; }
        public string CUST_ID { get; set; }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string PHONE_NO { get; set; }
        public string ADDRESS_1 { get; set; }
        public string ADDRESS_2 { get; set; }
        public string ADDRESS_3 { get; set; }
        public string CITY { get; set; }
        public string PRIMARY_FLAG { get; set; }
        public string ACTIVE { get; set; }
    }

    public class PostCustomer
    {
        [JsonProperty("origin_application")]
        public string origin_application { get; set; }

        [JsonProperty("store_sid")]
        public string store_sid { get; set; }

        [JsonProperty("last_name")]
        public string last_name { get; set; }

        [JsonProperty("first_name")]
        public string first_name { get; set; }

        [JsonProperty("customer_active")]
        public long customer_active { get; set; }

        [JsonProperty("customer_type")]
        public long customer_type { get; set; }

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("phones")]
        public List<Phone> phones { get; set; }

        [JsonProperty("address")]
        public List<BillingAddress> address { get; set; }
    }

    public partial class Phone
    {
        [JsonProperty("origin_application")]
        public string origin_application { get; set; }

        [JsonProperty("phone_no")]
        public string phone_no { get; set; }

        [JsonProperty("primary_flag")]
        public bool primary_flag { get; set; }

        [JsonProperty("seq_no")]
        public long seq_no { get; set; }
    }
    public partial class BillingAddress
    {
        [JsonProperty("origin_application")]
        public string origin_application { get; set; }

        [JsonProperty("customer_sid")]
        public string customer_sid { get; set; }

        //[JsonProperty("CUST_SID")]
        //public long CUST_SID { get; set; }

        [JsonProperty("primary_flag")]
        public bool primary_flag { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; }

        [JsonProperty("address_line_1")]
        public string address_line_1 { get; set; }

        [JsonProperty("address_line_2")]
        public string address_line_2 { get; set; }

        [JsonProperty("address_line_3")]
        public string address_line_3 { get; set; }

        //[JsonProperty("city")]
        //public string city { get; set; }

        [JsonProperty("address_line_4")]
        public object address_line_4 { get; set; }

        [JsonProperty("address_line_5")]
        public string address_line_5 { get; set; }

        [JsonProperty("address_line_6")]
        public object address_line_6 { get; set; }

        [JsonProperty("seq_no")]
        public long seq_no { get; set; }

        [JsonProperty("address_allow_contact")]
        public bool address_allow_contact { get; set; }
    }

    public class ItemInfo
    {
        public string SID { get; set; }
        public string UPC { get; set; }
        public string ALU { get; set; }
        public string DESCRIPTION1 { get; set; }
        public bool ACTIVE { get; set; }
    }
    public class ItemPostInfo
    {
        public string ORIGIN_APPLICATION { get; set; }//":"RProPrismWeb",  
        public string INVN_SBS_ITEM_SID { get; set; }//":"549369039000155118",
        public int? Order_Type { get; set; }//":0,
        public int? Item_Type { get; set; }//":3,
        public string FULFILL_STORE_SID { get; set; }//":549354565000159237,
        public int MANUAL_DISC_TYPE { get; set; }//":0,
        public Decimal MANUAL_DISC_VALUE { get; set; }


        //public long doc_sid { get; set; }//":"576450268000129095",
        //public int ITEM_POS { get; set; }//":"1",
        //public string INVN_SBS_ITEM_DESCRIPTION { get; set; }
        public long QUANTITY { get; set; }//":"10",
        //public string Scan_UPC { get; set; }//":"1000000015247",
        //public string Price_Lvl_Sid { get; set; }//":"533249320000159190"
        //public Decimal Item_Price { get; set; }
        //public bool Active { get; set; }
    }

    public partial class SODeposit
    {
        [JsonProperty("tenders")]
        public List<object> tenders { get; set; }

        [JsonProperty("so_deposit_amt_paid")]
        public string so_deposit_amt_paid { get; set; }
    }


    public class PostTender
    {
        public string ORIGIN_APPLICATION { get; set; }
        public string TENDER_TYPE { get; set; }
        public string DOCUMENT_SID { get; set; }
        public string TAKEN { get; set; }
        public string TENDER_NAME { get; set; }
        public string AUTHORIZATION_CODE { get; set; }
    }
    
    public class PostDocument
    {
      




        public string ORIGIN_APPLICATION { get; set; }//"ORIGIN_APPLICATION":"RPROPRISMWEB",
        public int ORDER_STATUS { get; set; }//"ORDER_STATUS":0,
        public int ORDER_TYPE { get; set; }//"ORDER_TYPE":0,
        public Decimal ORDER_QTY { get; set; }//"ORDER_QTY":10,
        public int ORDER_CHANGED_FLAG { get; set; }//"ORDER_CHANGED_FLAG":0,
        public int STATUS { get; set; }//"STATUS":0,
        public int IS_HELD { get; set; }//"IS_HELD":0,
        public int TENDER_TYPE { get; set; }//"TENDER_TYPE":-1,
        public string STORE_SID { get; set; }
        public string STORE_NO { get; set; }// "STORE_NO":2,
        public string STORE_NAME { get; set; }
        public string CUSTOMER_PO_NUMBER { get; set; }//"CUST_PO_NO":"BL860",
        public string BT_CUID { get; set; }//"BT_CUID":"549239131000133175",
        public string BT_ID { get; set; }      //"BT_ID":100000340,
        public string BT_COUNTRY { get; set; }
        //"BT_CUID":"549239131000133175",
        public string ST_ID { get; set; }      //"BT_ID":100000340,
        public string PRICE_LVL { get; set; }//"PRICE_LVL_NAME":"4",
        public Decimal TOTAL_LINE_ITEM { get; set; }//"TOTAL_LINE_ITEM":"1",
        public Decimal TOTAL_ITEM_COUNT { get; set; }//"TOTAL_ITEM_COUNT":"1",
        public int HAS_RETURN { get; set; }//"HAS_RETURN":"0",
        public int HAS_DEPOSIT { get; set; }//"HAS_DEPOSIT":"0"
        public string POS_FLAG1 { get; set; }
        public string UDF_STRING1 { get; set; }
        public string SUBSIDIARY_UID { get; set; }
        //public string STORE_NUMBER { get; set; }


        //public string ORDER_TYPE { get; set; }
        public string NOTES_GENERAL { get; set; }
        public string UDF_STRING2 { get; set; }
        public string UDF_STRING3 { get; set; }
        public string UDF_STRING4 { get; set; }
        public string UDF_STRING5 { get; set; }
        public string SHIP_DATE { get; set; }
        public string REF_ORDER_SID { get; set; }
        //public string STORE_UID { get; set; }
        public string POS_FLAG3 { get; set; }
        public string POS_FLAG2 { get; set; }
        public string TRACKING_NUMBER { get; set; }
        public string BT_ADDRESS_LINE1 { get; set; }
        public string BT_ADDRESS_LINE2 { get; set; }
        public string BT_ADDRESS_LINE3 { get; set; }
        public string BT_ADDRESS_LINE4 { get; set; }
        public string BT_ADDRESS_LINE5 { get; set; }
        public string ST_CUID { get; set; }
        public string ST_COUNTRY { get; set; }
        public string ST_ADDRESS_LINE1 { get; set; }
        public string ST_ADDRESS_LINE2 { get; set; }
        public string ST_ADDRESS_LINE3 { get; set; }
        public string ST_ADDRESS_LINE4 { get; set; }
        public string ST_ADDRESS_LINE5 { get; set; }
        //public string BT_ADDRESS_LINE6 { get; set; }
        //public string BT_POSTAL_CODE { get; set; }
        public bool SEND_SALE_FULFILLMENT { get; set; }

    }

    public partial class PutShippingInfo
    {
        public string ORDER_SHIP_METHOD_SID { get; set; }
        public string ORDER_SHIPPING_AMT_MANUAL { get; set; }
    }
    public partial class PutOrderDiscount
    {
        public string MANUAL_ORDER_DISC_TYPE { get; set; }
        public string MANUAL_ORDER_DISC_VALUE { get; set; }
        public string MANUAL_DISC_REASON { get; set; }
        public string MANUAL_ORDER_DISC_REASON { get; set; }
    }

    public class PutTaxInfo
    {
        public string TAX_AREA_NAME { get; set; }
    }

    public partial class PostResponceObject
    {

        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("row_version")]
        public long RowVersion { get; set; }

    }

    public class OrderStatusPut
    {
        public int ORDER_TYPE { get; set; }
        public int STATUS { get; set; }
    }

}