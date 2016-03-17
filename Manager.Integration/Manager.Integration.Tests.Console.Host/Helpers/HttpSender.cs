using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using Newtonsoft.Json;

namespace Manager.IntegrationTest.Console.Host.Helpers
{
	public class HttpSender : IHttpSender
	{
		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data)
		{
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					CreateRequestHeaders(client);

					this.Log().DebugWithLineNumber("Start Post Async: " + url);

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"))
							.ConfigureAwait(false);


					this.Log().DebugWithLineNumber("Finihed Post Async: " + url);

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

		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data,
		                                                 CancellationToken cancellationToken)
		{
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					CreateRequestHeaders(client);

					this.Log().DebugWithLineNumber("Start Post Async: " + url);

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"),
						                       cancellationToken)
							.ConfigureAwait(false);

					this.Log().DebugWithLineNumber("Finished Post Async: " + url);

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
					CreateRequestHeaders(client);

					this.Log().DebugWithLineNumber("Start Delete Async: " + url);

					var response =
						await client.DeleteAsync(url).ConfigureAwait(false);


					this.Log().DebugWithLineNumber("Finished Delete Async: " + url);

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

		public async Task<HttpResponseMessage> DeleteAsync(Uri url,
		                                                   CancellationToken cancellationToken)
		{
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					this.Log().DebugWithLineNumber("Start Delete Async: " + url);

					var response =
						await client.DeleteAsync(url,
						                         cancellationToken).ConfigureAwait(false);


					this.Log().DebugWithLineNumber("Finished Delete Async: " + url);

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
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					this.Log().DebugWithLineNumber("Start Get Async: " + url);

					var response = await client.GetAsync(url)
						.ConfigureAwait(false);


					this.Log().DebugWithLineNumber("Finished Get Async: " + url);

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

		public async Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken cancellationToken)
		{
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					this.Log().DebugWithLineNumber("Start Get Async: " + url);

					var response = await client.GetAsync(url,
					                                     cancellationToken)
						.ConfigureAwait(false);

					this.Log().DebugWithLineNumber("Finished Get Async: " + url);


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

		public async Task<bool> TryGetAsync(Uri url)
		{
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					this.Log().DebugWithLineNumber("Start Try Get Async: " + url);

					var response =
						await client.GetAsync(url)
							.ConfigureAwait(false);


					this.Log().DebugWithLineNumber("Finished Try Get Async: " + url);

					this.Log().DebugWithLineNumber("Finished.");

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

		public async Task<bool> TryGetAsync(Uri url, CancellationToken cancellationToken)
		{
			this.Log().DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					this.Log().DebugWithLineNumber("Start Try Get Async: " + url);

					var response =
						await client.GetAsync(url,
						                      cancellationToken)
							.ConfigureAwait(false);

					this.Log().DebugWithLineNumber("Finished Try Get Async: " + url);

					this.Log().DebugWithLineNumber("Finished.");

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

		private static void CreateRequestHeaders(HttpClient client)
		{
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}