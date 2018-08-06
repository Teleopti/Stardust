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
	public class ClearScheduleHandler : IHandle<ClearScheduleStardustEvent>
	{
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IHandleEvent<ClearScheduleStardustEvent> _realEventHandler;

		public ClearScheduleHandler(IDataSourceScope dataSourceScope, IStardustJobFeedback stardustJobFeedback, IHandleEvent<ClearScheduleStardustEvent> realEventHandler)
		{
			_dataSourceScope = dataSourceScope;
			_stardustJobFeedback = stardustJobFeedback;
			_realEventHandler = realEventHandler;
		}

		[AsSystem]
		public void Handle(ClearScheduleStardustEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			using (_dataSourceScope.OnThisThreadUse(parameters.LogOnDatasource))
			{
				_realEventHandler.Handle(parameters);
			}
			_stardustJobFeedback.SendProgress = null;
		}
	}
}