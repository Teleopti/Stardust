using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class QuickForecastSkillEvaluator : IQuickForecastSkillEvaluator
	{
		private readonly IQuickForecastWorkloadEvaluator _quickForecastWorkloadEvaluator;

		public QuickForecastSkillEvaluator(IQuickForecastWorkloadEvaluator quickForecastWorkloadEvaluator)
		{
			_quickForecastWorkloadEvaluator = quickForecastWorkloadEvaluator;
		}

		public ForecastingAccuracy[] Measure(ISkill skill, DateOnlyPeriod historicalPeriod)
		{
			return skill.WorkloadCollection.Select(workload => _quickForecastWorkloadEvaluator.Measure(workload, historicalPeriod)).ToArray();
		}
	}
}