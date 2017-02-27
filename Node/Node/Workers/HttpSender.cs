using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
	public class HttpSender : IHttpSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpSender));

		private const string Mediatype = "application/json";
		private static readonly HttpClient Client = new HttpClient();

		public HttpSender()
		{
			Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Mediatype));
		}

		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data)
		{
			return await PostAsync(url, data, CancellationToken.None);
		}

		public async Task<HttpResponseMessage> PostAsync(Uri url,
		                                                 object data,
		                                                 CancellationToken cancellationToken)
		{
			try
			{

				var sez = JsonConvert.SerializeObject(data);

				var response =
					await Client.PostAsync(url,
					                        new StringContent(sez, Encoding.Unicode, Mediatype),
					                        cancellationToken)
						.ConfigureAwait(false);
				return response;

			}
			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}
	}
}