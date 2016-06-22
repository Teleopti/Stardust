using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class OriginHandlerPipelineModule : HubPipelineModule
	{
		protected override bool OnBeforeAuthorizeConnect(HubDescriptor hubDescriptor, IRequest request)
		{
			var requestOrigin = request.Headers["Origin"];
			if (!string.IsNullOrWhiteSpace(requestOrigin))
			{
				var originUri = new Uri(requestOrigin);
				if (originUri.Host != request.Url.Host)
				{
					return false;
				}
			}
			return base.OnBeforeAuthorizeConnect(hubDescriptor, request);
		}
	}
}