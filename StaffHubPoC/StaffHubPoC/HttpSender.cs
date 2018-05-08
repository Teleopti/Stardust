using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace StaffHubPoC
{
	public class HttpSender
	{
		public static string Get(string endpoint, string token)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.BaseAddress = new Uri("https://api.manage.staffhub.office.com");

				var result = client.GetAsync(endpoint).Result;
				if (result.IsSuccessStatusCode)
				{
					var responseContent = result.Content;
					var responseString = responseContent.ReadAsStringAsync().Result;
					return responseString;
				}
				else
				{
					Console.WriteLine("Crap.");
				}

				return null;
			}
		}
	}
}
