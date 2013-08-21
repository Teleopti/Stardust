using System;
using System.Web.Routing;
using Contrib.SignalR.SignalRMessageBus;
using Microsoft.AspNet.SignalR;

namespace Teleopti.Ccc.Web.Broker
{
	public static class SignalRConfiguration
	{
		public static Action<HubConfiguration> MapHubs = c => RouteTable.Routes.MapHubs(c);

		public static ActionThrottle ActionThrottle; 

		public static void Configure(HubConfiguration hubConfiguration)
		{
			var settingsFromParser = TimeoutSettings.Load();

			if (settingsFromParser.DefaultMessageBufferSize.HasValue)
				GlobalHost.Configuration.DefaultMessageBufferSize = settingsFromParser.DefaultMessageBufferSize.Value;

			if (settingsFromParser.DisconnectTimeout.HasValue)
				GlobalHost.Configuration.DisconnectTimeout = settingsFromParser.DisconnectTimeout.Value;

			if (settingsFromParser.KeepAlive.HasValue)
				GlobalHost.Configuration.KeepAlive = settingsFromParser.KeepAlive.Value;

			if (settingsFromParser.ConnectionTimeout.HasValue)
				GlobalHost.Configuration.ConnectionTimeout = settingsFromParser.ConnectionTimeout.Value;

			MapHubs(hubConfiguration);

			if (settingsFromParser.ScaleOutBackplaneUrl != null)
			{
				GlobalHost.DependencyResolver.UseSignalRServer(settingsFromParser.ScaleOutBackplaneUrl);
			}

			ActionThrottle = new ActionThrottle(settingsFromParser.MessagesPerSecond);
			ActionThrottle.Start();
		}
	}
}