using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public interface IQueueStatisticsCalculator
    {
        void Calculate(IStatisticTask statisticTask);
    }
}