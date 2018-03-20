using System.Net.Http;
using Microsoft.Owin.Testing;

namespace Teleopti.Wfm.Api.Test
{
	public abstract class ApiTest
	{
		private readonly TestServer _server;

		protected HttpClient Client { get; }

		protected ApiTest()
		{
			// Arrange
			_server = TestServer.Create<Startup>();
			Client = _server.HttpClient;
		}

		protected void Authorize()
		{
			Client.DefaultRequestHeaders.Add("Authorization", "bearer afdsafasdf");
		}
	}


}
