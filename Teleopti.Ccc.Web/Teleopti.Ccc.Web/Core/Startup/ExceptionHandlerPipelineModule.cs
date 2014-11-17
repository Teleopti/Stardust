using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[CLSCompliant(false)]
	public class ExceptionHandlerPipelineModule : HubPipelineModule
	{
		protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
		{
			invokerContext.Hub.Clients.Caller.ExceptionHandler(exceptionContext.Error.Message);
		}
	}
}