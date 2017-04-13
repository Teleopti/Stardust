using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.WinCode.Reporting
{
    [Serializable]
    public class ReportSettingsScheduledTimePerActivity : SettingValue
    {
        public ReportSettingsScheduledTimePerActivity()
        {
            GroupPage = string.Empty;
            EndDate = DateTime.Today;
            StartDate = DateTime.Today;
            Persons = new List<Guid>();
            Activities = new List<Guid>();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<Guid> Persons { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<Guid> Activities { get; set; }

        public Guid? Scenario { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string GroupPage { get; set; }
    }
}