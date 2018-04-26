﻿using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public static class JsonExtensions
	{
		public static T ExecuteJsonRequest<T>(this UriBuilder uriBuilder)
		{
			var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
			request.AllowAutoRedirect = true;

			return responseToJson<T>(request);
		}

		public static T PostRequest<T>(this HttpWebRequest request, string json)
		{
			request.AllowAutoRedirect = false;
			request.Method = "POST";
			request.ContentType = "application/json";
			request.Accept = "application/json";

			using (var requestWriter = new StreamWriter(request.GetRequestStream()))
			{
				requestWriter.Write(json);
			}

			request.AllowAutoRedirect = true;
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
			T returnValue = default(T);
			Retry.Handle<WebException>()
				.WaitAndRetry(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10))
				.Do(() =>
				{
					using (var response = request.GetResponse())
					{
						using (var reader = new StreamReader(response.GetResponseStream()))
						{
							var jsonString = reader.ReadToEnd();
							returnValue = JsonConvert.DeserializeObject<T>(jsonString);
						}
					}
				});

			return returnValue;
		}
	}
}