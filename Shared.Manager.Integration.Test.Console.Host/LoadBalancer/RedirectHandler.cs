using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Manager.IntegrationTest.Console.Host.Log4Net;

namespace Manager.IntegrationTest.Console.Host.LoadBalancer
{
	public class RedirectHandler : DelegatingHandler
    {
        private readonly HttpClient client = new HttpClient();
        
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			this.Log().DebugWithLineNumber("Start Send Async.");

            try
            {
                var host = RoundRobin.Next(request);

                request.RequestUri = new Uri(host,
                    new Uri(request.RequestUri.GetComponents(UriComponents.SchemeAndServer,
                        UriFormat.Unescaped)).MakeRelativeUri(request.RequestUri));

                request.Headers.Host = null;

                if (request.Method.Equals(HttpMethod.Get))
                {
                    request.Content = null;
                }

                var response = await client.SendAsync(request,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken);

                response.Headers.Add("SourceHost", host.ToString());

                return response;
            }
            catch (Exception e)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.Message)
                };
            }
        }
	}
}