using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class ExceptionHandlerPipelineModule : HubPipelineModule
	{
		protected override void OnIncomingError(Exception ex, IHubIncomingInvokerContext context)
		{
			context.Hub.Clients.Caller.ExceptionHandler(ex.Message);
		}
	}
}