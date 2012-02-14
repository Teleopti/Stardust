using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public interface IQuickForecastPeriodProvider
    {
        DateOnlyPeriod DefaultStatisticPeriod { get; }
        DateOnlyPeriod DefaultTargetPeriod { get; }
    }
}