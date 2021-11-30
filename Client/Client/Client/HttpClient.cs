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
        /*
        public HttpClient()
            {
                endpoint = string.Empty;
                httpMethod = httpVerbs.GET;
                users = new List<string>();
            }
            public enum httpVerbs
            {
                GET,
                POST,
                DELETE,
                PUT
            }
            public string[] MakeRequest(string message, int type)
            {
            string msgResponse = string.Empty;
            message = message.Replace("http", "https");
            if (type == 0)
            {
                httpMethod = httpVerbs.GET;
            }
            if (type == 1)
            {
                httpMethod = httpVerbs.POST;
            }
            if (type == 2)
            {
                httpMethod = httpVerbs.DELETE;
            }
            if (type == 3)
            {
                httpMethod = httpVerbs.PUT;
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(message);

            request.Method = httpMethod.ToString();
            request.ServerCertificateValidationCallback += (a, b, c, d) => true;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    //GDY RESPONSE NIE JEST 200 OK
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        //TODO
                        if (response.StatusCode != HttpStatusCode.NotFound)
                        {
                            if (type == 0)
                            {
                                string[] resp = new string[] { "NO" };
                                return resp;

                            }
                        }
                        string[] stringResponse = new string[] { "" };
                        stringResponse[0] = "NAK";

                        return stringResponse;
                        throw new Exception("Error code: " + response.StatusCode);


                    }
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (StreamReader reader = new StreamReader(responseStream))
                            {
                                String[] resp = new String[1000];
                                String jsonResponse = "";
                                int i = 0;
                                jsonResponse = reader.ReadLine();
                                while (jsonResponse != null)
                                {

                                    resp[i] = jsonResponse;
                                    i++;
                                    jsonResponse = reader.ReadLine();
                                }

                                return resp;

                            }
                        }
                        else
                        {
                            return new string[0];
                        }

                    }
                }

            }
            catch (System.Net.WebException)
            {
                if (message == "https://" + apiAddress + ":" + apiPort + "/table/" + tableCode)
                {
                    String[] resp = new String[1];
                    resp[0] = "NO";
                    return resp;
                }
                else
                {
                    String[] resp = new String[2];
                    resp[0] = "";
                    resp[1] = "Error";
                    return resp;
                }

            }

        }*/
    }
}
