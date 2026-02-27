using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace JULKE
{
    public static class RetailProApiCall
    {

        #region Prism APIs

        public static async Task<RestResponse> GetAsync(string targetLink)
        {
            try
            {
                var client = new RestClient(targetLink);
                var request = new RestRequest("", Method.Get);
                request.AddHeader("Auth-Session", AppVariables.RetailProAuthSession);
                request.AddHeader("Accept", "application/Json,version=2.0");
                RestResponse response = await client.ExecuteAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static RestResponse Post(string targetLink, object requestBody)
        {
            try
            {
                var client = new RestClient(targetLink);
                //client.Timeout = -1;
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Auth-Session", AppVariables.RetailProAuthSession);
                request.AddHeader("Accept", "application/Json,version=2.0");
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(requestBody);
                var xy = JsonConvert.SerializeObject(requestBody);
                RestResponse response = client.ExecuteAsync(request).Result;

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static RestResponse PUT(string targetLink, object requestBody)
        {
            try
            {
                var client = new RestClient(targetLink);
                //client.Timeout = -1;
                var request = new RestRequest("", Method.Put);
                request.AddHeader("Auth-Session", AppVariables.RetailProAuthSession);
                request.AddHeader("Accept", "application/Json,version=2.0");
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(requestBody);
                RestResponse response = client.ExecuteAsync(request).Result;

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        #endregion

    }
}