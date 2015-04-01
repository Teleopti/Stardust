using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IQuickForecastEvaluator
	{
		IEnumerable<SkillAccuracy> MeasureForecastForAllSkills();
	}
}