using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PerformanceTests
{
	public class HttpSender 
	{
		
		public async Task<HttpResponseMessage> PostAsync(Uri url,
																			object data)
		{
			using (var client = new HttpClient())
			{
				var sez = JsonConvert.SerializeObject(data);

				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				HttpResponseMessage response =
					await client.PostAsync(url,
						new StringContent(sez,
							Encoding.Unicode,
							"application/json"))
						.ConfigureAwait(false);

					
				return response;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var response = await client.DeleteAsync(url)
					.ConfigureAwait(false);

					
				return response;
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var response = await client.GetAsync(url,
					HttpCompletionOption.ResponseHeadersRead)
					.ConfigureAwait(false);

				return response;
			}
		}

		public async Task<bool> TryGetAsync(Uri url)
		{
			try
			{
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					HttpResponseMessage response =
						 await client.GetAsync(url).ConfigureAwait(false);

					return response.IsSuccessStatusCode;
				}
			}

			catch (HttpRequestException exp)
			{
				
			}

		
			return false;
		}
	}
}