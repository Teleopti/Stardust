using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
	public class HttpSender : IHttpSender
	{
		private const string Mediatype = "application/json";
		private static readonly HttpClient Client = new HttpClient();

		public HttpSender()
		{
			Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Mediatype));
		}

		public async Task<HttpResponseMessage> PostAsync(Uri url,
			object data,
			CancellationToken cancellationToken)
		{
			var retries = 3;
			while (true)
			{
				try
				{
					return await Client.PostAsync(url,
							new StringContent(JsonConvert.SerializeObject(data, new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Ignore}), Encoding.Unicode, Mediatype),
							cancellationToken)
						.ConfigureAwait(false);
				}
				catch
				{
					if (--retries == 0) throw;
					await Task.Delay(50, cancellationToken);
				}
			}
		}
	}
}