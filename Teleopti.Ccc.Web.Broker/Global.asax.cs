using System;
using System.Web;
using System.Web.Routing;
using Contrib.SignalR.SignalRMessageBus;
using Contrib.SignalR.SignalRMessageBus.Backend;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;

namespace Teleopti.Ccc.Web.Broker
{
	public class Global : HttpApplication
	{

		protected void Application_Start(object sender, EventArgs e)
		{
			log4net.Config.XmlConfigurator.Configure();

			var settingsFromParser = TimeoutSettings.Load();

			if (settingsFromParser.BackendServerUrl != null)
			{
				GlobalHost.DependencyResolver = new TeleoptiDependencyResolver();
				GlobalHost.DependencyResolver.UseSignalRServer(settingsFromParser.BackendServerUrl);
			}

			if (settingsFromParser.HeartbeatInterval.HasValue)
				GlobalHost.Configuration.HeartbeatInterval = settingsFromParser.HeartbeatInterval.Value;

			if (settingsFromParser.DisconnectTimeout.HasValue)
				GlobalHost.Configuration.DisconnectTimeout = settingsFromParser.DisconnectTimeout.Value;

			if (settingsFromParser.KeepAlive.HasValue)
				GlobalHost.Configuration.KeepAlive = settingsFromParser.KeepAlive.Value;

			if (settingsFromParser.ConnectionTimeout.HasValue)
				GlobalHost.Configuration.ConnectionTimeout = settingsFromParser.ConnectionTimeout.Value;

			GlobalHost.HubPipeline.EnableAutoRejoiningGroups();

			RouteTable.Routes.MapConnection<SignalRBackplane>("backplane", "backplane"); 

			RouteTable.Routes.MapHubs();
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

	internal class TeleoptiDependencyResolver : DefaultDependencyResolver
	{
		private bool _resolveDefaultMessageBus;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public override object GetService(Type serviceType)
		{
			if (serviceType == typeof(SignalRBackplane))
			{
				_resolveDefaultMessageBus = true;
			}

			if (_resolveDefaultMessageBus && serviceType == typeof(IMessageBus))
			{
				Register(typeof(MessageBus), () => new MessageBus(this));
				_resolveDefaultMessageBus = false;
				return base.GetService(typeof(MessageBus));
			}

			return base.GetService(serviceType);
		}
	}
}