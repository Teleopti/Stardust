using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IQuickForecastSkillEvaluator
	{
		SkillAccuracy Measure(ISkill skill, DateOnlyPeriod historicalPeriod);
	}
}