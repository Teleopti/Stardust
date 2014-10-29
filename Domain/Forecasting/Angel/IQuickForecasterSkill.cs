using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecasterSkill
	{
		void Execute(ISkill skill, DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod);
	}
}