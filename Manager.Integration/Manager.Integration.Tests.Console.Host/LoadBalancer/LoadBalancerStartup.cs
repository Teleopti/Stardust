using System.Web.Http;
using Owin;

namespace Manager.IntegrationTest.Console.Host.LoadBalancer
{
	public class LoadBalancerStartup
	{
		public void Configuration(IAppBuilder appBuilder)
		{
			HttpConfiguration config = new HttpConfiguration();

			config.MessageHandlers.Clear();
			config.MessageHandlers.Add(new RedirectHandler());

			appBuilder.UseWebApi(config);
		}
	}
}