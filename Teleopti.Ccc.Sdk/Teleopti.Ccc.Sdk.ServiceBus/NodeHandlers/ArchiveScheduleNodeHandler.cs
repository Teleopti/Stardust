using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ArchiveScheduleNodeHandler : IHandle<ArchiveScheduleEvent>
	{
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IHandleEvent<ArchiveScheduleEvent> _realEventHandler;

		public ArchiveScheduleNodeHandler(IDataSourceScope dataSourceScope, IStardustJobFeedback stardustJobFeedback, IHandleEvent<ArchiveScheduleEvent> realEventHandler)
		{
			_dataSourceScope = dataSourceScope;
			_stardustJobFeedback = stardustJobFeedback;
			_realEventHandler = realEventHandler;
		}

		[AsSystem]
		public void Handle(ArchiveScheduleEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress,
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