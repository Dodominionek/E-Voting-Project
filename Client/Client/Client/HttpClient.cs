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
//using System.Text.Json;
//using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace HttpClient
{
    static class HttpClient
    {
            public static RestClient restClient = new RestClient("http://127.0.0.1:5000/");
            public static IRestResponse MakeRequest(RestRequest restRequest )
            {

                var result =restClient.Execute(restRequest);
                return result;
            }
      
    }
}
