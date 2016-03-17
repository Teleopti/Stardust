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
			Logger.DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					CreateRequestHeaders(client);

					Logger.DebugWithLineNumber("Start Post Async: " + url);

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"))
							.ConfigureAwait(false);

					Logger.DebugWithLineNumber("Finihed Post Async: " + url);

					Logger.DebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);
				throw;
			}
		}

		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data,
		                                                 CancellationToken cancellationToken)
		{
			Logger.DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					var sez = JsonConvert.SerializeObject(data);

					CreateRequestHeaders(client);

					Logger.DebugWithLineNumber("Start Post Async: " + url);

					var response =
						await client.PostAsync(url,
						                       new StringContent(sez,
						                                         Encoding.Unicode,
						                                         "application/json"),
						                       cancellationToken)
							.ConfigureAwait(false);

					Logger.DebugWithLineNumber("Finished Post Async: " + url + "\n" + "Finished");

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
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);
				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			Logger.DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.DebugWithLineNumber("Start Delete Async: " + url);

					var response =
						await client.DeleteAsync(url).ConfigureAwait(false);

					Logger.DebugWithLineNumber("Finished Delete Async: " + url);

					Logger.DebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(Uri url,
		                                                   CancellationToken cancellationToken)
		{
			Logger.DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.DebugWithLineNumber("Start Delete Async: " + url);

					var response =
						await client.DeleteAsync(url,
						                         cancellationToken).ConfigureAwait(false);

					Logger.DebugWithLineNumber("Finished Delete Async: " + url);

					Logger.DebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url)
		{
			Logger.DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.DebugWithLineNumber("Start Get Async: " + url);

					var response = await client.GetAsync(url,
					                                     HttpCompletionOption.ResponseHeadersRead)
						.ConfigureAwait(false);

					Logger.DebugWithLineNumber("Finished Get Async: " + url);

					Logger.DebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken cancellationToken)
		{
			Logger.DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.DebugWithLineNumber("Start Get Async: " + url);

					var response = await client.GetAsync(url,
					                                     HttpCompletionOption.ResponseHeadersRead,
					                                     cancellationToken)
						.ConfigureAwait(false);

					Logger.DebugWithLineNumber("Finished Get Async: " + url);


					Logger.DebugWithLineNumber("Finished.");

					return response;
				}
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}
		}

		public async Task<bool> TryGetAsync(Uri url)
		{
			Logger.DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.DebugWithLineNumber("Start Try Get Async: " + url);

					var response =
						await client.GetAsync(url,
						                      HttpCompletionOption.ResponseHeadersRead)
							.ConfigureAwait(false);


					Logger.DebugWithLineNumber("Finished Try Get Async: " + url);

					Logger.DebugWithLineNumber("Finished.");

					return response.IsSuccessStatusCode;
				}
			}

			catch (HttpRequestException exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}

			Logger.DebugWithLineNumber("Return false");
			return false;
		}

		public async Task<bool> TryGetAsync(Uri url, CancellationToken cancellationToken)
		{
			Logger.DebugWithLineNumber("Start.");

			try
			{
				using (var client = new HttpClient())
				{
					CreateRequestHeaders(client);

					Logger.DebugWithLineNumber("Start Try Get Async: " + url);

					var response =
						await client.GetAsync(url,
						                      HttpCompletionOption.ResponseHeadersRead,
						                      cancellationToken)
							.ConfigureAwait(false);

					Logger.DebugWithLineNumber("Finished Try Get Async: " + url);

					Logger.DebugWithLineNumber("Finished.");

					return response.IsSuccessStatusCode;
				}
			}

			catch (HttpRequestException exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber(exp.Message,
				                                 exp);

				throw;
			}

			Logger.DebugWithLineNumber("Return false");
			return false;
		}

		private static void CreateRequestHeaders(HttpClient client)
		{
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}