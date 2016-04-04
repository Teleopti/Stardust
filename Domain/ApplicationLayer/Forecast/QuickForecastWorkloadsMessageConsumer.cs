using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public class QuickForecastWorkloadsMessageConsumer : IHandleEvent<QuickForecastWorkloadsMessage>, IRunOnServiceBus
	{
		private readonly IEventPublisher _serviceBus;


		public QuickForecastWorkloadsMessageConsumer(IEventPublisher serviceBus)
		{
			_serviceBus = serviceBus;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(QuickForecastWorkloadsMessage message)
		{
			var messages = message.WorkloadIds.Select(workloadId => new QuickForecastWorkloadMessage
				{
					LogOnBusinessUnitId = message.LogOnBusinessUnitId, LogOnDatasource = message.LogOnDatasource, JobId = message.JobId, 
					ScenarioId = message.ScenarioId, StatisticPeriod = message.StatisticPeriod, TargetPeriod = message.TargetPeriod, 
					WorkloadId = workloadId, TemplatePeriod = message.TemplatePeriod,
					SmoothingStyle = message.SmoothingStyle, IncreaseWith = message.IncreaseWith,
                    UseDayOfMonth = message.UseDayOfMonth
				}).ToList();
			messages.ForEach(m => _serviceBus.Publish(m));
		}
	}
}