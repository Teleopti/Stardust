using System.Web.Http;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;
using Owin;

namespace Manager.IntegrationTest.Console.Host.LoadBalancer
{
	public class LoadBalancerStartup
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof(LoadBalancerStartup));

		public void Configuration(IAppBuilder appBuilder)
		{
			LogHelper.LogDebugWithLineNumber(Logger,"Start.");

			HttpConfiguration config = new HttpConfiguration();

			config.MessageHandlers.Clear();
			config.MessageHandlers.Add(new RedirectHandler());

			appBuilder.UseWebApi(config);

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");
		}
	}
}