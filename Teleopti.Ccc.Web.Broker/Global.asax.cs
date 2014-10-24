using System;
using System.Web;

namespace Teleopti.Ccc.Web.Broker
{
	public class Global : HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
		}

		protected void Application_End(object sender, EventArgs e)
		{
			if (SignalRConfiguration.ActionScheduler is IDisposable)
			{
				var actionThrottle = SignalRConfiguration.ActionScheduler as ActionThrottle;
				if (actionThrottle != null) actionThrottle.Dispose();
			}
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