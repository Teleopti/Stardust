using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting
{
    public class ReportSettingsScheduledTimePerActivityModel
    {
        private IScenario _scenario;
        private DateOnlyPeriod _period;
        private IList<IPerson> _persons;
        private TimeZoneInfo _timeZone;
        private IList<IActivity> _activities;

        public IScenario Scenario
        {
            get { return _scenario; }
            set { _scenario = value; }
        }

        public DateOnlyPeriod Period
        {
            get { return _period; }
            set { _period = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<IPerson> Persons
        {
            get { return _persons; }
            set { _persons = value; }
        }

        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
            set { _timeZone = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<IActivity> Activities
        {
            get { return _activities; }
            set { _activities = value; }
        }

    }
}
