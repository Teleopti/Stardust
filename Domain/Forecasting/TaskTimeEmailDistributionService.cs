using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class TaskTimeEmailDistributionService : ITaskTimeDistributionService
    {
        #region ITaskTimeDistributionService Members

        public DistributionType DistributionType
        {
            get { return DistributionType.Even; }
        }

        #endregion
    }
}