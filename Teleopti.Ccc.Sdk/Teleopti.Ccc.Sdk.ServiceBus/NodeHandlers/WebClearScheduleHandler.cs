using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class WebClearScheduleHandler : IHandle<WebClearScheduleStardustEvent>
	{
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IHandleEvent<WebClearScheduleStardustEvent> _realEventHandler;

		public WebClearScheduleHandler(IStardustJobFeedback stardustJobFeedback, IHandleEvent<WebClearScheduleStardustEvent> realEventHandler)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_realEventHandler = realEventHandler;
		}

		public void Handle(WebClearScheduleStardustEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			_realEventHandler.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}