using System.Web.Http;
using Manager.IntegrationTest.Console.Host.Log4Net;
using Owin;

namespace Manager.IntegrationTest.Console.Host.LoadBalancer
{
	public class LoadBalancerStartup
	{
        public void Configuration(IAppBuilder appBuilder)
        {
            this.Log().DebugWithLineNumber("Start.");

            var config = new HttpConfiguration();

            config.MessageHandlers.Clear();
            config.MessageHandlers.Add(new RedirectHandler());

            appBuilder.UseWebApi(config);

            this.Log().DebugWithLineNumber("Finished.");
        }
    }
}