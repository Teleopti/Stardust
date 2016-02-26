using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Newtonsoft.Json;

namespace Manager.IntegrationTest.Console.Host.Helpers
{
	public class HttpSender : IHttpSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (HttpSender));

		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					CreateRequestHeaders(client);

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"))
							.ConfigureAwait(false);

					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);
				throw;
			}
		}

		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data,
		                                                 CancellationToken cancellationToken)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					CreateRequestHeaders(client);

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"),
						                       cancellationToken)
							.ConfigureAwait(false);

					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);
				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					var response =
						await client.DeleteAsync(url).ConfigureAwait(false);

					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url,
		                                                   CancellationToken cancellationToken)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					var response =
						await client.DeleteAsync(url, cancellationToken).ConfigureAwait(false);

					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
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
					CreateRequestHeaders(client);

					var response = await client.GetAsync(url,
					                                     HttpCompletionOption.ResponseHeadersRead)
						.ConfigureAwait(false);

					return response;
				}
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken cancellationToken)
		{
			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					var response = await client.GetAsync(url,
					                                     HttpCompletionOption.ResponseHeadersRead,
					                                     cancellationToken)
						.ConfigureAwait(false);

					return response;
				}
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
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
					CreateRequestHeaders(client);

					var response =
						await client.GetAsync(url,
						                      HttpCompletionOption.ResponseHeadersRead)
							.ConfigureAwait(false);

					return response.IsSuccessStatusCode;
				}
			}

			catch (HttpRequestException exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);

				throw;
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Return false");
			return false;
		}

		public async Task<bool> TryGetAsync(Uri url, CancellationToken cancellationToken)
		{
			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					var response =
						await client.GetAsync(url,
						                      HttpCompletionOption.ResponseHeadersRead,
						                      cancellationToken)
							.ConfigureAwait(false);

					return response.IsSuccessStatusCode;
				}
			}

			catch (HttpRequestException exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);

				throw;
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Return false");
			return false;
		}

		private static void CreateRequestHeaders(HttpClient client)
		{
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}