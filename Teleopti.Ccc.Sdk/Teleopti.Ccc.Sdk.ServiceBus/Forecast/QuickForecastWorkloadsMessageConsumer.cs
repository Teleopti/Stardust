using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class QuickForecastWorkloadsMessageConsumer : ConsumerOf<QuickForecastWorkloadsMessage>
	{
		private readonly IServiceBus _serviceBus;

		public QuickForecastWorkloadsMessageConsumer(IServiceBus serviceBus)
		{
			_serviceBus = serviceBus;
		}

		public void Consume(QuickForecastWorkloadsMessage message)
		{
			var messages = message.WorkloadIds.Select(workloadId => new QuickForecastWorkloadMessage
				{
					BusinessUnitId = message.BusinessUnitId, Datasource = message.Datasource, JobId = message.JobId, 
					ScenarioId = message.ScenarioId, StatisticPeriod = message.StatisticPeriod, TargetPeriod = message.TargetPeriod, 
					UpdateStandardTemplates = message.UpdateStandardTemplates, WorkloadId = workloadId
				}).ToList();
			messages.ForEach(m => _serviceBus.Send(m));
		}
	}
}