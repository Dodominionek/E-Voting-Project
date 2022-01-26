using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RestSharp;
using Newtonsoft;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using RestSharp.Authenticators;

namespace HttpClient
{
    //TDL
    //Close add voting
    //Clear boxes after view change.
    static class HttpClient
    {
        public static RestClient restClient = new RestClient("https://127.0.0.1:443/");

        public static IRestResponse MakeRequest(RestRequest restRequest)
        {
            restClient.Proxy = new WebProxy();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var result = restClient.Execute(restRequest);
            return result;
        }
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                string hash = GetHash(sha256Hash, password);
                return hash;
            }
        }
        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {


            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}


