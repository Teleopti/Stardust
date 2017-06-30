using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class RunSchedulingHandler : IHandle<ScheduleOnNode>
	{
		private readonly IComponentContext _componentContext;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly SchedulingProgress _feedback;
		private Action<string> _sendProgress;

		public RunSchedulingHandler(IComponentContext componentContext, ISchedulingProgress feedback,
											 IDataSourceScope dataSourceScope)
		{
			_componentContext = componentContext;
			_dataSourceScope = dataSourceScope;
			_feedback = feedback as SchedulingProgress;
		}

		private void feedbackFeedbackChanged(object sender, FeedbackEventArgs e)
		{
			if(!string.IsNullOrEmpty(e.JobResultDetail.Message))
				_sendProgress(e.JobResultDetail.Message);
		}

		[AsSystem]
		public virtual void Handle(ScheduleOnNode @event, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			_feedback.FeedbackChanged += feedbackFeedbackChanged;
			_sendProgress = sendProgress;
			using (_dataSourceScope.OnThisThreadUse(@event.LogOnDatasource))
			{
				var theRealOne = _componentContext.Resolve<IFullScheduling>();
				var period = new DateOnlyPeriod(new DateOnly(@event.StartDate), new DateOnly(@event.EndDate));
				theRealOne.DoScheduling(period);
				_feedback.FeedbackChanged -= feedbackFeedbackChanged;

			}
		}
	}
}