using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	[UseNotOnToggle(Toggles.Wfm_QuickForecastOnHangfire_35845)]
	public class QuickForecastWorkloadsEventHandlerServiceBus : QuickForecastWorkloadsEventHandlerBase, IHandleEvent<QuickForecastWorkloadsEvent>, IRunOnServiceBus
	{
		public QuickForecastWorkloadsEventHandlerServiceBus(IQuickForecastWorkloadProcessor quickForecastWorkloadProcessor) : base(quickForecastWorkloadProcessor)
		{
		}

		public new void Handle(QuickForecastWorkloadsEvent @event)
		{
			base.Handle(@event);
		}
	}

	[UseOnToggle(Toggles.Wfm_QuickForecastOnHangfire_35845), AsSystem]
	public class QuickForecastWorkloadsEventHandlerHangfire : QuickForecastWorkloadsEventHandlerBase, IHandleEvent<QuickForecastWorkloadsEvent>, IRunOnHangfire
	{
		public QuickForecastWorkloadsEventHandlerHangfire(IQuickForecastWorkloadProcessor quickForecastWorkloadProcessor) : base(quickForecastWorkloadProcessor)
		{
		}

		public new void Handle(QuickForecastWorkloadsEvent @event)
		{
			base.Handle(@event);
		}
	}

	[AsSystem]
	public class QuickForecastWorkloadsEventHandlerBase
	{
		private readonly IQuickForecastWorkloadProcessor _quickForecastWorkloadProcessor;
		public QuickForecastWorkloadsEventHandlerBase(IQuickForecastWorkloadProcessor quickForecastWorkloadProcessor)
		{
			_quickForecastWorkloadProcessor = quickForecastWorkloadProcessor;
		}
		public void Handle(QuickForecastWorkloadsEvent @event)
		{
			var messages = @event.WorkloadIds.Select(workloadId => new QuickForecastWorkloadEvent
			{
				LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
				LogOnDatasource = @event.LogOnDatasource,
				JobId = @event.JobId,
				ScenarioId = @event.ScenarioId,
				StatisticPeriod = @event.StatisticPeriod,
				TargetPeriod = @event.TargetPeriod,
				WorkloadId = workloadId,
				TemplatePeriod = @event.TemplatePeriod,
				SmoothingStyle = @event.SmoothingStyle,
				IncreaseWith = @event.IncreaseWith,
				UseDayOfMonth = @event.UseDayOfMonth
			}).ToList();
			messages.ForEach(m => _quickForecastWorkloadProcessor.Handle(m));
		}
	}
}