using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class RunSchedulingHandler : IHandle<ScheduleOnNode>
	{
		private readonly IComponentContext _componentContext;
		private readonly SchedulingProgress _feedback;
		private Action<string> _sendProgress;

		public RunSchedulingHandler(IComponentContext componentContext, ISchedulingProgress feedback)
		{
			_componentContext = componentContext;
			_feedback = feedback as SchedulingProgress;
		}

		private void feedbackFeedbackChanged(object sender, FeedbackEventArgs e)
		{
			if(!string.IsNullOrEmpty(e.JobResultDetail.Message))
				_sendProgress(e.JobResultDetail.Message);
		}

		public void Handle(ScheduleOnNode parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			_feedback.FeedbackChanged += feedbackFeedbackChanged;
			_sendProgress = sendProgress;
			//var theRealOne = _componentContext.Resolve<FullScheduling>();

			_feedback.FeedbackChanged -= feedbackFeedbackChanged;
		}
	}
}