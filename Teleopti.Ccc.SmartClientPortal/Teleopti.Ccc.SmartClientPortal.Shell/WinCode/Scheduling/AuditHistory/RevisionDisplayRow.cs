using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory
{
    public class RevisionDisplayRow
    {
        public string Name { get; set; }
        public DateTime ChangedOn { get; set; }
        public IScheduleDay ScheduleDay{ get; set; }
    }
}