using System;

namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    public interface IWeekHeaderCellData
    {
        TimeSpan MinimumWeekWorkTime { get; }
        TimeSpan MaximumWeekWorkTime { get; }
        bool Validated { get; }
        bool Invalid { get; }
        bool Alert { get; }
        int WeekNumber { get; }
    }
}
