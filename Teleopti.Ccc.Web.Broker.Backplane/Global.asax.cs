using System;
using System.Web;
using Contrib.SignalR.SignalRMessageBus.Backend;

namespace Teleopti.Ccc.Web.Broker.Backplane
{
	public class Global : HttpApplication
    {
		protected void Application_Start(object sender, EventArgs e)
        {
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
		    var storage = new IdStorage();
            storage.OnStop();
		}
	}
}