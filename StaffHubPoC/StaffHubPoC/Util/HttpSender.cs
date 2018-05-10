using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace StaffHubPoC
{
	public class HttpSender
	{

		public static string Post(string endpoint, string payload, string token)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.BaseAddress = new Uri("https://api.manage.staffhub.office.com");
				var stringContent = new StringContent(payload, Encoding.UTF8, "application/json");
				var result = client.PostAsync(endpoint, stringContent).Result;
				if (result.IsSuccessStatusCode)
				{
					var responseContent = result.Content;
					var responseString = responseContent.ReadAsStringAsync().Result;
					return responseString;
				}

				Console.WriteLine("Crap.");
				return null;

			}
		}

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

				Console.WriteLine("Crap.");
				return null;
			}
		}

		public static string Delete(string endpoint, string payload, string token)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				var request = new HttpRequestMessage
				{
					Content = new StringContent(payload, Encoding.UTF8, "application/json"),
					Method = HttpMethod.Delete,
					RequestUri = new Uri("https://api.manage.staffhub.office.com/" + endpoint)
					//RequestUri = new Uri("[YOUR URL GOES HERE]")
				};
				

				var result = client.SendAsync(request).Result;
				if (result.IsSuccessStatusCode)
				{
					var responseContent = result.Content;
					var responseString = responseContent.ReadAsStringAsync().Result;
					return responseString;
				}

				Console.WriteLine("Crap.");
				return null;
			}
		}
	}
}
