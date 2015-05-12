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

		public SkillAccuracy Measure(ISkill skill, DateOnlyPeriod historicalPeriod)
		{
			return new SkillAccuracy
			{
				Id = skill.Id.Value,
				Name = skill.Name,
				Workloads = skill.WorkloadCollection.Select(workload => _quickForecastWorkloadEvaluator.Measure(workload, historicalPeriod)).ToArray()
			};
		}
	}
}