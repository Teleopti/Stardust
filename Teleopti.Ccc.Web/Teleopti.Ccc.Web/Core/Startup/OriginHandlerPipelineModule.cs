using System;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class OriginHandlerPipelineModule : HubPipelineModule
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(OriginHandlerPipelineModule));

		protected override bool OnBeforeAuthorizeConnect(HubDescriptor hubDescriptor, IRequest request)
		{
			var requestOrigin = request.Headers["Origin"];
			if (!string.IsNullOrWhiteSpace(requestOrigin))
			{
				var originUri = new Uri(requestOrigin);
				if (originUri.Host != request.Url.Host)
				{
					if (Logger.IsDebugEnabled)
					{
						Logger.InfoFormat("A request with origin {0} was found but expected {1}",originUri.Host,request.Url.Host);
					}
					return false;
				}
			}
			return base.OnBeforeAuthorizeConnect(hubDescriptor, request);
		}
	}
}