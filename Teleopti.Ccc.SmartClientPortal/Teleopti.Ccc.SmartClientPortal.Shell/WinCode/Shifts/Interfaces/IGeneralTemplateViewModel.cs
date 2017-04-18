using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IGeneralTemplateViewModel : IBaseModel
    {
        string Accessibility { get; set; }

        IActivity BaseActivity { get; set; }

        IShiftCategory Category { get; set; }

        TimeSpan StartPeriodStartTime { get; set; }

        TimeSpan StartPeriodEndTime { get; set; }

        TimeSpan StartPeriodSegment { get; set; }

        TimeSpan EndPeriodStartTime { get; set; }

        TimeSpan EndPeriodEndTime { get; set; }

        TimeSpan EndPeriodSegment { get; set; }

        TimeSpan WorkingSegment { get; set; }

        TimeSpan WorkingStartTime { get; set; }

        TimeSpan WorkingEndTime { get; set; }

		bool OnlyForRestrictions { get; set; }
    }
}
