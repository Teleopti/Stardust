using System;
using System.Web;
using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class Global : HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
		}

		protected void Application_End(object sender, EventArgs e)
		{
			var actionScheduler = Startup._container.Resolve<IActionScheduler>();
			if (actionScheduler is IDisposable)
				(actionScheduler as IDisposable).Dispose();
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

	}
}