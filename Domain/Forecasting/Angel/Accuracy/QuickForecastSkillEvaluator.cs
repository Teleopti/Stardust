using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class QuickForecastSkillEvaluator : IQuickForecastSkillEvaluator
	{
		private readonly IForecastWorkloadEvaluator _forecastWorkloadEvaluator;

		public QuickForecastSkillEvaluator(IForecastWorkloadEvaluator forecastWorkloadEvaluator)
		{
			_forecastWorkloadEvaluator = forecastWorkloadEvaluator;
		}

		public SkillAccuracy Measure(ISkill skill)
		{
			return new SkillAccuracy
			{
				Id = skill.Id.Value,
				Workloads = skill.WorkloadCollection.Select(workload => _forecastWorkloadEvaluator.Evaluate(workload)).ToArray()
			};
		}
	}
}