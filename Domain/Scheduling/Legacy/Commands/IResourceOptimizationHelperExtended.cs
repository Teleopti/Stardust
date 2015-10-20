using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public interface IResourceOptimizationHelperExtended
    {
        void ResourceCalculateAllDays(IBackgroundWorkerWrapper backgroundWorker);
	    void ResourceCalculateMarkedDays(IBackgroundWorkerWrapper backgroundWorker, bool considerShortBreaks);
    }
}