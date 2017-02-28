using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AuditHistory
{
    public class RevisionDisplayRow
    {
        public string Name { get; set; }
        public DateTime ChangedOn { get; set; }
        public IScheduleDay ScheduleDay{ get; set; }
    }
}