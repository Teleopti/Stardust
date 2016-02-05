using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public interface IResourceOptimizationHelperExtended
    {
        void ResourceCalculateAllDays(IBackgroundWorkerWrapper backgroundWorker, bool doIntraIntervalCalculation);
	    void ResourceCalculateMarkedDays(IBackgroundWorkerWrapper backgroundWorker, bool considerShortBreaks, bool doIntraIntervalCalculation);
    }
}