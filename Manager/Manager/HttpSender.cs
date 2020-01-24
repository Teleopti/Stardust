using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Manager.Interfaces;

namespace Stardust.Manager
{
	public class HttpSender : IHttpSender
	{
		private const string Mediatype = "application/json";
		private static readonly HttpClient Client = new HttpClient();
		private const int RetryDelayMilliseconds = 50;
		private const int RetryCount = 3;

		public HttpSender()
		{
			Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Mediatype));
		}

		public async Task<HttpResponseMessage> PutAsync(Uri url, object data)
		{
			var retries = RetryCount;
			while (true)
			{
				try
				{
					return await Client.PutAsync(url,
							new StringContent(JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), Encoding.Unicode, Mediatype))
						.ConfigureAwait(false);
				}
				catch
				{
					if (--retries == 0) throw;
					await Task.Delay(RetryDelayMilliseconds);
				}
			}
		}


		public async Task<HttpResponseMessage> PostAsync(Uri url, object data)
		{
			var retries = RetryCount;
			while (true)
			{
				try
				{
					return await Client.PostAsync(url,
							new StringContent(JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), Encoding.Unicode, Mediatype))
						.ConfigureAwait(false);
				}
				catch
				{
					if (--retries == 0) throw;
					await Task.Delay(RetryDelayMilliseconds);
				}
			}
		}

		public async Task<HttpResponseMessage> GetAsync(Uri url)
		{
			var retries = RetryCount;
			while (true)
			{
				try
				{
					return await Client.GetAsync(url,HttpCompletionOption.ResponseContentRead)
						.ConfigureAwait(false);
				}
				catch
				{
					if (--retries == 0) throw;
					await Task.Delay(RetryDelayMilliseconds);
				}
			}
		}


		public async Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			var retries = RetryCount;
			while (true)
			{
				try
				{
					return await Client.DeleteAsync(url)
						.ConfigureAwait(false);
				}
				catch
				{
					if (--retries == 0) throw;
					await Task.Delay(RetryDelayMilliseconds);
				}
			}
		}
	}
}