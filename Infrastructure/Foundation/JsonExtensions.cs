using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public static class JsonExtensions
	{
		public static T ExecuteJsonRequest<T>(this UriBuilder uriBuilder)
		{
			var request = (HttpWebRequest)HttpWebRequest.Create(uriBuilder.ToString());
			request.AllowAutoRedirect = false;
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