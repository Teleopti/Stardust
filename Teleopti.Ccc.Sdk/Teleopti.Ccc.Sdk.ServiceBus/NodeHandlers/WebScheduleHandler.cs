using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
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
		private readonly IComponentContext _componentContext;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public WebScheduleHandler(IStardustJobFeedback stardustJobFeedback, IComponentContext componentContext, IDataSourceScope dataSourceScope)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_componentContext = componentContext;
			_dataSourceScope = dataSourceScope;
		}

		[AsSystem]
		public virtual void Handle(WebScheduleStardustEvent parameters, 
			CancellationTokenSource cancellationTokenSource, 
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			//_stardustJobFeedback.SendProgress = sendProgress;
			using (_dataSourceScope.OnThisThreadUse(parameters.LogOnDatasource))
			{
				var theRealOne = _componentContext.Resolve<IHandleEvent<WebScheduleStardustEvent>>();
				theRealOne.Handle(parameters);
			}
			_stardustJobFeedback.SendProgress = null;
		}
	}
}