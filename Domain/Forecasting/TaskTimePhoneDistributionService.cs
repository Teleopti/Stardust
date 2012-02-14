using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class TaskTimePhoneDistributionService : ITaskTimeDistributionService
    {
        public DistributionType DistributionType
        {
            get { return DistributionType.ByPercent; }
        }
    }
}