using System;
using Contrib.SignalR.SignalRMessageBus;
using Microsoft.AspNet.SignalR;
using Owin;

namespace Teleopti.Ccc.Web.Broker
{
	public static class SignalRConfiguration
	{
		public static IActionScheduler ActionScheduler; 

		public static void Configure(Func<IAppBuilder> mapSignalR)
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

			if (settingsFromParser.ScaleOutBackplaneUrl != null)
			{
				GlobalHost.DependencyResolver.UseSignalRServer(settingsFromParser.ScaleOutBackplaneUrl);
			}

			mapSignalR();

			if (settingsFromParser.ThrottleMessages)
			{
				var actionThrottle = new ActionThrottle(settingsFromParser.MessagesPerSecond);
				actionThrottle.Start();
				ActionScheduler = actionThrottle;
			}
			else
			{
				ActionScheduler = new ActionImmediate();
			}
		}
	}
}