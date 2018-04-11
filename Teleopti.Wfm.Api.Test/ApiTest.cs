using System.Net.Http;
using Autofac;
using Microsoft.Owin.Testing;

namespace Teleopti.Wfm.Api.Test
{
	public abstract class ApiTest
	{
		private readonly TestServer _server;

		protected HttpClient Client { get; }

		protected ApiTest()
		{
			var startup = new Startup(testRegistrations);
			_server = TestServer.Create(x => startup.Configuration(x));
			Client = _server.HttpClient;
		}

		private void testRegistrations(ContainerBuilder obj)
		{
			obj.RegisterType<FakeTokenVerifier>().As<ITokenVerifier>();
		}

		protected void Authorize()
		{
			Client.DefaultRequestHeaders.Add("Authorization", "bearer afdsafasdf");
		}
	}
}
