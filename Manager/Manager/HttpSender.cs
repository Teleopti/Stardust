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
		private const string Mediatype = "application/json";
		private static readonly HttpClient Client = new HttpClient();

		public HttpSender()
		{
			Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Mediatype));
		}

		public async Task<HttpResponseMessage> PutAsync(Uri url, object data)
		{
			try
			{
				var sez = JsonConvert.SerializeObject(data);

				var response =
					await Client.PutAsync(url, new StringContent(sez, Encoding.Unicode, Mediatype))
						.ConfigureAwait(false);

				return response;

			}
			catch (Exception exp)
			{
				this.Log().InfoWithLineNumber(exp.Message);
				return null;
			}
		}


		public async Task<HttpResponseMessage> PostAsync(Uri url, object data)
		{
			try
			{
				var sez = JsonConvert.SerializeObject(data);

				var response =
					await Client.PostAsync(url, new StringContent(sez, Encoding.Unicode, Mediatype))
						.ConfigureAwait(false);

				return response;

			}
			catch (Exception exp)
			{
				this.Log().InfoWithLineNumber(exp.Message);
				return null;
			}
		}


		public async Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			try
			{
				var response = await Client.DeleteAsync(url)
					.ConfigureAwait(false);

				return response;

			}
			catch (Exception exp)
			{
				this.Log().InfoWithLineNumber(exp.Message);
				return null;
			}
		}
	}
}