using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;

namespace JULKE
{
    public static class RetailProAuthentication
    {
        public static string GetSession(string user, string password)
        {
            string authSessionId = string.Empty;
            string workStation = ConfigurationManager.AppSettings["Workstation"].ToString();
            string serverIp = ConfigurationManager.AppSettings["ServerWebAddress"].ToString();
            try
            {
                var baseUrl = serverIp; // "https://" +  + "/";
                var client = new RestClient(baseUrl + "/v1/rest/auth");

                var authNonceRequest = new RestRequest("", Method.Get);

                authNonceRequest.AddHeader("Accept", "application/Json,version=2.0");
                var authNonceResponse = client.ExecuteAsync(authNonceRequest).Result;
                var xxx = authNonceResponse.Content;

                var authNonce = Convert.ToDecimal(authNonceResponse.Headers.Where(w => w.Name != null && w.Name.Equals("Auth-Nonce"))
                    .Select(s => s.Value).FirstOrDefault());

                var authNonceValue = (Math.Truncate(authNonce / 13) % 99999) * 17;
                //=============================================================================================================> Acquire Auth-Session Token
                
                client = new RestClient(baseUrl + "/v1/rest/auth?usr=" + user + "&pwd=" + password);

                var authSessionRequest = new RestRequest("", Method.Get);
                authSessionRequest.AddHeader("Auth-Nonce", authNonce.ToString(CultureInfo.InvariantCulture));
                authSessionRequest.AddHeader("Auth-Nonce-Response", authNonceValue.ToString(CultureInfo.InvariantCulture));
                authSessionRequest.AddHeader("Accept", "application/Json,version=2.0");
                var authSessionResponse = client.ExecuteAsync(authSessionRequest).Result;

                if (authSessionResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    authSessionId = authSessionResponse.Headers.Where(w => w.Name != null && w.Name.Equals("Auth-Session"))
                        .Select(s => s.Value).FirstOrDefault()
                        ?.ToString();

                    client = new RestClient(baseUrl + "/v1/rest/sit?ws=" + workStation);
                    var seatRequest = new RestRequest("", Method.Get);
                    seatRequest.AddHeader("Auth-Session", authSessionId ?? string.Empty);
                    seatRequest.AddHeader("Accept", "application/Json,version=2.0");
                    var authResponse = client.ExecuteAsync(seatRequest).Result;
                }
                //=============================================================================================================> Acquire Seat

                if (!string.IsNullOrEmpty(authSessionId))
                    return authSessionId;
                return "Error";
            }
            catch (Exception)
            {
                return null;
            }
        }

        //public static bool VacateSeat(string serverIp, string workStation)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }

        //    return false;
        //}
    }

    public class AcquireSeatInfo
    {
        public string AuthSession { get; set; }
    }
}