using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IResourceOptimizationHelperWin
    {
        void ResourceCalculateAllDays(BackgroundWorker backgroundWorker, bool useOccupancyAdjustment);
    }
}