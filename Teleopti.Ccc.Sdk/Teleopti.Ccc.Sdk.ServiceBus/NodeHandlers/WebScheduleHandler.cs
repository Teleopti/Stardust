using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class WebScheduleHandler : IHandle<WebScheduleStardustEvent>
	{
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IHandleEvent<WebScheduleStardustEvent> _realEventHandler;

		public WebScheduleHandler(IStardustJobFeedback stardustJobFeedback, IHandleEvent<WebScheduleStardustEvent> realEventHandler, IDataSourceScope dataSourceScope)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_realEventHandler = realEventHandler;
			_dataSourceScope = dataSourceScope;
		}

		public virtual void Handle(WebScheduleStardustEvent parameters, 
			CancellationTokenSource cancellationTokenSource, 
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			_realEventHandler.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}