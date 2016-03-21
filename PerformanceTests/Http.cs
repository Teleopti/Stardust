using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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
			try
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

			catch (Exception exp)
			{
				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			
			try
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

			catch (Exception exp)
			{
				throw;
			}
		}

		public void GetAsync(Uri url)
		{
			try
			{
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = client.GetAsync(url,
										HttpCompletionOption.ResponseHeadersRead)
							  .ConfigureAwait(false);

					
				}
			}

			catch (Exception exp)
			{
				throw;
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

			catch (Exception exp)
			{
				
				throw;
			}

			
			return false;
		}
	}
}