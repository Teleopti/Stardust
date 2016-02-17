using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public interface IResourceOptimizationHelperExtended
    {
        void ResourceCalculateAllDays(ISchedulingProgress backgroundWorker, bool doIntraIntervalCalculation);
	    void ResourceCalculateMarkedDays(ISchedulingProgress backgroundWorker, bool considerShortBreaks, bool doIntraIntervalCalculation);
    }
}