using System;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;

namespace Teleopti.Ccc.Web.Broker
{
	public class Global : HttpApplication
	{

		protected void Application_Start(object sender, EventArgs e)
		{
			log4net.Config.XmlConfigurator.Configure();

			var settingsFromParser = TimeoutSettings.Load();

			if (settingsFromParser.DefaultMessageBufferSize.HasValue)
				GlobalHost.Configuration.DefaultMessageBufferSize = settingsFromParser.DefaultMessageBufferSize.Value;
 
			if (settingsFromParser.DisconnectTimeout.HasValue)
				GlobalHost.Configuration.DisconnectTimeout = settingsFromParser.DisconnectTimeout.Value;

			if (settingsFromParser.KeepAlive.HasValue)
				GlobalHost.Configuration.KeepAlive = settingsFromParser.KeepAlive.Value;

			if (settingsFromParser.ConnectionTimeout.HasValue)
				GlobalHost.Configuration.ConnectionTimeout = settingsFromParser.ConnectionTimeout.Value;

			RouteTable.Routes.MapHubs(new HubConfiguration{EnableCrossDomain = true});
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}