using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public interface IQueueStatisticsCalculator
    {
        void Calculate(IStatisticTask statisticTask);
    }
}