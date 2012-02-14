using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IResourceOptimizationHelperWin
    {
        void ResourceCalculateAllDays(DoWorkEventArgs e, BackgroundWorker backgroundWorker, bool useOccupancyAdjustment);
        void OnResourcesChanged(IEnumerable<DateOnly> changedDays);
    }
}