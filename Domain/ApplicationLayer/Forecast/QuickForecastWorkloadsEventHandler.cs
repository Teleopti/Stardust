using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public class QuickForecastWorkloadsEventHandler : IHandleEvent<QuickForecastWorkloadsEvent>, IRunOnServiceBus
	{
		private readonly IQuickForecastWorkloadProcessor _quickForecastWorkloadProcessor;

		public QuickForecastWorkloadsEventHandler(QuickForecastWorkloadProcessor quickForecastWorkloadProcessor)
		{
			_quickForecastWorkloadProcessor = quickForecastWorkloadProcessor;
		}
		
		public void Handle(QuickForecastWorkloadsEvent @event)
		{
			var messages = @event.WorkloadIds.Select(workloadId => new QuickForecastWorkloadEvent
				{
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId, LogOnDatasource = @event.LogOnDatasource, JobId = @event.JobId, 
					ScenarioId = @event.ScenarioId, StatisticPeriod = @event.StatisticPeriod, TargetPeriod = @event.TargetPeriod, 
					WorkloadId = workloadId, TemplatePeriod = @event.TemplatePeriod,
					SmoothingStyle = @event.SmoothingStyle, IncreaseWith = @event.IncreaseWith,
                    UseDayOfMonth = @event.UseDayOfMonth
				}).ToList();
			messages.ForEach(m => _quickForecastWorkloadProcessor.Handle(m));
		}
	}
}