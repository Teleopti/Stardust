﻿using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Stardust.Node.Extensions
{
    public static class JsonExtensions
    {
        public static T ExecuteJsonRequest<T>(this UriBuilder uriBuilder)
        {
            var request = (HttpWebRequest) WebRequest.Create(uriBuilder.Uri);
            request.AllowAutoRedirect = true;

            return responseToJson<T>(request);
        }

        public static T PostRequest<T>(this HttpWebRequest request, string json)
        {
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.Write(json);
            }
            return responseToJson<T>(request);
        }

        public static T GetRequest<T>(this HttpWebRequest request)
        {
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.AllowAutoRedirect = true;

            return responseToJson<T>(request);
        }

        private static T responseToJson<T>(WebRequest request)
        {
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonString = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(jsonString);
                }
            }
        }
    }
}