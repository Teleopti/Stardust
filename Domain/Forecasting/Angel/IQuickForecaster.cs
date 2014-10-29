using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		void Execute(ISkill skill, DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod);
	}
}