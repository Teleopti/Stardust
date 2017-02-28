using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IQuickForecastSkillEvaluator
	{
		SkillAccuracy Measure(ISkill skill);
	}
}