using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IActivityTimeLimiterViewModel : IBaseModel<ActivityTimeLimiter>
    {
        IActivity TargetActivity { get; set; }

        TimeSpan Time { get; set; }

        string Operator { get; set; }
    }
}
