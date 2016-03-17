using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
	public class HttpSender : IHttpSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (HttpSender));

		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data)
		{
			Logger.LogDebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					CreateRequestHeaders(client);

					Logger.LogDebugWithLineNumber("Start Post Async: " + url);

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"))
							.ConfigureAwait(false);

					Logger.LogDebugWithLineNumber("Finihed Post Async: " + url);

					Logger.LogDebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);
				throw;
			}
		}

		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data,
		                                                 CancellationToken cancellationToken)
		{
			Logger.LogDebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					CreateRequestHeaders(client);

					Logger.LogDebugWithLineNumber("Start Post Async: " + url);

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"),
						                       cancellationToken)
							.ConfigureAwait(false);

					Logger.LogDebugWithLineNumber("Finished Post Async: " + url + "\n" + "Finished");

					return response;
				}
			}
			catch (HttpRequestException)
			{
				//don't log these annoying "errors" when only the node is up
				throw;
			}

			catch (Exception exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);
				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			Logger.LogDebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.LogDebugWithLineNumber("Start Delete Async: " + url);

					var response =
						await client.DeleteAsync(url).ConfigureAwait(false);

					Logger.LogDebugWithLineNumber("Finished Delete Async: " + url);

					Logger.LogDebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url,
		                                                   CancellationToken cancellationToken)
		{
			Logger.LogDebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.LogDebugWithLineNumber("Start Delete Async: " + url);

					var response =
						await client.DeleteAsync(url,
						                         cancellationToken).ConfigureAwait(false);

					Logger.LogDebugWithLineNumber("Finished Delete Async: " + url);

					Logger.LogDebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url)
		{
			Logger.LogDebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.LogDebugWithLineNumber("Start Get Async: " + url);

					var response = await client.GetAsync(url,
					                                     HttpCompletionOption.ResponseHeadersRead)
						.ConfigureAwait(false);

					Logger.LogDebugWithLineNumber("Finished Get Async: " + url);

					Logger.LogDebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken cancellationToken)
		{
			Logger.LogDebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.LogDebugWithLineNumber("Start Get Async: " + url);

					var response = await client.GetAsync(url,
					                                     HttpCompletionOption.ResponseHeadersRead,
					                                     cancellationToken)
						.ConfigureAwait(false);

					Logger.LogDebugWithLineNumber("Finished Get Async: " + url);


					Logger.LogDebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<bool> TryGetAsync(Uri url)
		{
			Logger.LogDebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.LogDebugWithLineNumber("Start Try Get Async: " + url);

					var response =
						await client.GetAsync(url,
						                      HttpCompletionOption.ResponseHeadersRead)
							.ConfigureAwait(false);


					Logger.LogDebugWithLineNumber("Finished Try Get Async: " + url);

					Logger.LogDebugWithLineNumber("Finished.");

					return response.IsSuccessStatusCode;
				}
			}

			catch (HttpRequestException exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);
			}

			catch (Exception exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}

			Logger.LogDebugWithLineNumber("Return false");
			return false;
		}

		public async Task<bool> TryGetAsync(Uri url, CancellationToken cancellationToken)
		{
			Logger.LogDebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.LogDebugWithLineNumber("Start Try Get Async: " + url);

					var response =
						await client.GetAsync(url,
						                      HttpCompletionOption.ResponseHeadersRead,
						                      cancellationToken)
							.ConfigureAwait(false);

					Logger.LogDebugWithLineNumber("Finished Try Get Async: " + url);

					Logger.LogDebugWithLineNumber("Finished.");

					return response.IsSuccessStatusCode;
				}
			}

			catch (HttpRequestException exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);
			}

			catch (Exception exp)
			{
				Logger.LogErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}

			Logger.LogDebugWithLineNumber("Return false");
			return false;
		}

		private static void CreateRequestHeaders(HttpClient client)
		{
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}