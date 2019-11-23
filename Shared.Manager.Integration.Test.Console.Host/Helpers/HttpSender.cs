using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Manager.IntegrationTest.ConsoleHost.Log4Net;
using Newtonsoft.Json;

namespace Manager.IntegrationTest.ConsoleHost.Helpers
{
	public class HttpSender 
	{
        private readonly HttpClient client = new HttpClient();

		public async Task<HttpResponseMessage> PostAsync(Uri url, object data = null)
		{
            try
            {
                var sez = data == null ? "" : JsonConvert.SerializeObject(data);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response =
                    await client.PostAsync(url,
                            new StringContent(sez,
                                Encoding.Unicode,
                                "application/json"))
                        .ConfigureAwait(false);
                return response;
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
            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response =
                    await client.DeleteAsync(url).ConfigureAwait(false);
                return response;
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
            var response = await client.GetAsync(url).ConfigureAwait(false);
            return response;
        }
    }
}