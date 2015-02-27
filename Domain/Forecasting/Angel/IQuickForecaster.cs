using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		double Execute(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
	}
}