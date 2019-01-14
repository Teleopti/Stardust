using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class CopyScheduleNodeHandler : IHandle<CopyScheduleEvent>
	{
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IHandleEvent<CopyScheduleEvent> _realEventHandler;

		public CopyScheduleNodeHandler(IStardustJobFeedback stardustJobFeedback, IHandleEvent<CopyScheduleEvent> realEventHandler)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_realEventHandler = realEventHandler;
		}

		public void Handle(CopyScheduleEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			_realEventHandler.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}