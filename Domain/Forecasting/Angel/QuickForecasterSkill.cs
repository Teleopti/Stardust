using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecasterSkill : IQuickForecasterSkill
	{
		private readonly IQuickForecasterWorkload _quickForecasterWorkload;

		public QuickForecasterSkill(IQuickForecasterWorkload quickForecasterWorkload)
		{
			_quickForecasterWorkload = quickForecasterWorkload;
		}

		public void Execute(ISkill skill, DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod)
		{
			foreach (var workload in skill.WorkloadCollection)
			{
				_quickForecasterWorkload.Execute(workload, historicalPeriod, futurePeriod);
			}
		}
	}
}