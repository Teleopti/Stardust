using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.DynamicProxy2;
using log4net;
using Newtonsoft.Json;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
	[Intercept("log-calls")]
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

					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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

					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
	}
}