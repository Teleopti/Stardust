using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ImportScheduleNodeHandler : IHandle<ImportScheduleEvent>
	{
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IHandleEvent<ImportScheduleEvent> _realEventHandler;

		public ImportScheduleNodeHandler(IStardustJobFeedback stardustJobFeedback, IHandleEvent<ImportScheduleEvent> realEventHandler)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_realEventHandler = realEventHandler;
		}

		public void Handle(ImportScheduleEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			_realEventHandler.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}