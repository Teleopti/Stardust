using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IActivityTimeLimiterViewModel : IBaseModel<ActivityTimeLimiter>
    {
        IActivity TargetActivity { get; set; }

        TimeSpan Time { get; set; }

        string Operator { get; set; }
    }
}
