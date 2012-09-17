using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Reporting
{
     [Serializable]
    public class ReportSettingsScheduleAuditing :SettingValue
    {
        public ReportSettingsScheduleAuditing()
        {
            ChangeStartDate = DateOnly.Today;
            ChangeEndDate = DateOnly.Today;
            ScheduleStartDate = DateOnly.Today;
            ScheduleEndDate = DateOnly.Today;
            User = new Guid();
            GroupPage = string.Empty;
            Agents = new HashSet<Guid>();
        }

        public DateOnly ChangeStartDate { get; set; }

        public DateOnly ChangeEndDate { get; set; }

        public DateOnly ScheduleStartDate { get; set; }

        public DateOnly ScheduleEndDate { get; set; }

        public string GroupPage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public HashSet<Guid> Agents { get; set; }

        public Guid User { get; set; }
    }
}