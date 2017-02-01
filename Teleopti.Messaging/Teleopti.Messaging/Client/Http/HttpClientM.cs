using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Messaging.Client.Http
{
	// MMMMmmMm as in Roger.
	// No, seriously, just couldnt use HttpClient and did want to call it HttpClient2 ROFL
	public class HttpClientM
	{
		private readonly IHttpServer _server;
		private readonly IMessageBrokerUrl _url;

		public HttpClientM(IHttpServer server, IMessageBrokerUrl url)
		{
			_server = server;
			_url = url;
		}

		public void Post(string call, object thing)
		{
			var u = url(call);
			_server.Post(u, thing);
		}
		
		public string GetOrThrow(string call)
		{
			var u = url(call);
			return _server.GetOrThrow(u);
		}

		private string url(string call)
		{
			if (string.IsNullOrEmpty(_url?.Url))
				return null;
			return _url.Url.TrimEnd('/') + "/" + call;
		}
	}
}