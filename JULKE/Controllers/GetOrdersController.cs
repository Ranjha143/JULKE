using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Configuration;
using Newtonsoft.Json;

namespace JULKE.Controllers
{

    public class PushWebOrdersController : ApiController
    {
        [HttpPost]
        [Route("api/v1/ShopifyWebHook")]
        public async Task<HttpResponseMessage> Post()   //[FromBody] JObject jObject
        {
            try
            {
                JObject jObject = JsonConvert.DeserializeObject<JObject>(await Request.Content.ReadAsStringAsync());// (JObject) await Request.Content.ReadAsStringAsync();

                string connectionString = AppVariables.MongoConnectionString;
                MongoClient dbClient = new MongoClient(connectionString);
                var database = dbClient.GetDatabase(AppVariables.MongoDatabase);


                var collection = database.GetCollection<BsonDocument>("Orders");

                if (jObject != null)
                {
                    var shopifyOrderNo = jObject?["name"]?.ToString();
                    if (string.IsNullOrEmpty(shopifyOrderNo))
                    {
                        
                    }
                    else {
                        BsonDocument document = BsonDocument.Parse(jObject.ToString());

                        document.Add("isPosted", false);
                        document.Add("hasError", false);

                        var filter = Builders<BsonDocument>.Filter.Eq("name", shopifyOrderNo);
                        var existingDocument = await collection.Find(filter).FirstOrDefaultAsync();
                        if (existingDocument == null)
                        {
                            await collection.InsertOneAsync(document);
                        }

                        // =======================================================================================
                        var path = HttpContext.Current.Server.MapPath("~") + "\\incomingOrders\\already_exist\\";
                        if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                        System.IO.File.WriteAllText(path + shopifyOrderNo + ".txt", jObject?.ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                var path = HttpContext.Current.Server.MapPath("~") + "\\incomingOrders\\";

                File.AppendAllText(path + "expection.log", ex.Message + Environment.NewLine + JsonConvert.SerializeObject(ex));

                JObject jObject = JsonConvert.DeserializeObject<JObject>(await Request.Content.ReadAsStringAsync());
                // =======================================================================================

                

                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                System.IO.File.AppendAllText(path + "ErrorOrders.txt", jObject?.ToString() + Environment.NewLine + " ================ " + Environment.NewLine);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "", Configuration.Formatters.JsonFormatter);
        }

        [HttpPost]
        [Route("api/v1/CancelWebHook")]
        public async Task<HttpResponseMessage> PostCancel()   //[FromBody] JObject jObject
        {
            try
            {
                JObject jObject = JsonConvert.DeserializeObject<JObject>(await Request.Content.ReadAsStringAsync());// (JObject) await Request.Content.ReadAsStringAsync();


                string connectionString = AppVariables.MongoConnectionString;
                MongoClient dbClient = new MongoClient(connectionString);
                var database = dbClient.GetDatabase(AppVariables.MongoDatabase);

                var collection = database.GetCollection<BsonDocument>("OrdersCanceled");

                if (jObject != null)
                {
                    var shopifyOrderNo = jObject?["name"].ToString();
                    BsonDocument document = BsonDocument.Parse(jObject.ToString());
                    document.Add("isPosted", false); 
                    document.Add("hasError", false);

                    var filter = Builders<BsonDocument>.Filter.Eq("name", shopifyOrderNo);
                    var existingDocument = await collection.Find(filter).FirstOrDefaultAsync();
                    if (existingDocument == null)
                    {
                        await collection.InsertOneAsync(document);
                    }

                    // =======================================================================================
                    var path = HttpContext.Current.Server.MapPath("~") + "\\incomingOrders\\CancelWebHook\\";
                    if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                    System.IO.File.WriteAllText(path + shopifyOrderNo + ".txt", jObject?.ToString());

                }
            }
            catch (Exception ex)
            {
                var path = HttpContext.Current.Server.MapPath("~") + "\\incomingOrders\\CancelWebHook\\";

                File.AppendAllText(path + "expection.log", ex.Message + Environment.NewLine + JsonConvert.SerializeObject(ex));

                JObject jObject = JsonConvert.DeserializeObject<JObject>(await Request.Content.ReadAsStringAsync());
                // =======================================================================================



                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                System.IO.File.AppendAllText(path + "ErrorOrders.txt", jObject?.ToString() + Environment.NewLine + " ================ " + Environment.NewLine);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "", Configuration.Formatters.JsonFormatter);
        }
    }

    public class SystemSettingsController : ApiController
    {
        [HttpGet]
        [Route("Getpath")]
        public async Task<HttpResponseMessage> GetPath()
        {
            await Task.Delay(0);

            var path = HttpContext.Current.Server.MapPath("~") + "incomingOrders\\";

            return Request.CreateResponse(HttpStatusCode.OK, path, Configuration.Formatters.JsonFormatter);
        }
    }

    public class CityModel
    {
        [JsonProperty("name")]
        public string CityName { get; set; } //name
    }


    public static class StringExtension
    {
        public static string GetLast(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }
    }
}
