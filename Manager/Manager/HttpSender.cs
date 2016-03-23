using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Manager.Extensions;
using Stardust.Manager.Interfaces;

namespace Stardust.Manager
{
	public class HttpSender : IHttpSender
	{
		/// <summary>
		/// </summary>
		/// <param name="url"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data)
		{
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"))
							.ConfigureAwait(false);

					this.Log().DebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message,
				                               exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await client.DeleteAsync(url)
						.ConfigureAwait(false);

					this.Log().DebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message,
				                               exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url)
		{
			try
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

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message,
				                               exp);

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

					var response =
						await client.GetAsync(url).ConfigureAwait(false);

					return response.IsSuccessStatusCode;
				}
			}

			catch (HttpRequestException exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message,
				                               exp);
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message,
				                               exp);

				throw;
			}

			this.Log().DebugWithLineNumber("Return false");

			return false;
		}
	}
}