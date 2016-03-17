using System.Web.Http;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using Owin;

namespace Manager.IntegrationTest.Console.Host.LoadBalancer
{
	public class LoadBalancerStartup
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof(LoadBalancerStartup));

		public void Configuration(IAppBuilder appBuilder)
		{
			Logger.LogDebugWithLineNumber("Start.");

			HttpConfiguration config = new HttpConfiguration();

			config.MessageHandlers.Clear();
			config.MessageHandlers.Add(new RedirectHandler());

			appBuilder.UseWebApi(config);

			Logger.LogDebugWithLineNumber("Finished.");
		}
	}
}