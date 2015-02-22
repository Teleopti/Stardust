using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IResourceOptimizationHelperWin
    {
        void ResourceCalculateAllDays(IBackgroundWorkerWrapper backgroundWorker, bool useOccupancyAdjustment);
    }
}