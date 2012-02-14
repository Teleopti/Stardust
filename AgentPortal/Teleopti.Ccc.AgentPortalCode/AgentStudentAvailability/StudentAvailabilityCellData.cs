using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public interface IStudentAvailabilityCellData
    {
        string TipText { get; set; }
        EffectiveRestriction EffectiveRestriction { get; set; }
        TimeSpan WeeklyMax { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<StudentAvailabilityRestriction> StudentAvailabilityRestrictions { get; set; }

        DateTime TheDate { get; set; }
        bool IsInsidePeriod { get; set; }
        TimeSpan PeriodTarget { get; set; }
        bool Enabled { get; set; }
        TimeSpan BalancedPeriodTarget { get; set; }
        Color DisplayColor { get; set; }
        bool Legal { get; set; }
        bool HasDayOff { get; set; }
        bool HasShift { get; set; }
        string DisplayName { get; set; }
        string DisplayShortName { get; set; }
        bool HasAbsence { get; set; }
        bool HasPersonalAssignmentOnly { get; set; }
        bool HasPreference { get; set; }
    }

    public class StudentAvailabilityCellData : IStudentAvailabilityCellData
    {
        private IList<StudentAvailabilityRestriction> _studentAvailabilityRestrictions;

        public StudentAvailabilityCellData()
        {
            Enabled = true;
        }

        public EffectiveRestriction EffectiveRestriction{get;set;}

        public TimeSpan WeeklyMax{get; set;}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<StudentAvailabilityRestriction> StudentAvailabilityRestrictions
        {
            get { return _studentAvailabilityRestrictions; }
            set
            {
                if (Enabled)
                {
                    EffectiveRestriction = null;
                    _studentAvailabilityRestrictions = value;
                }
            }
        }

        public DateTime TheDate{get;set;}

        public bool IsInsidePeriod{get;set;}

        public TimeSpan PeriodTarget{get;set;}

        public bool Enabled{get;set;}

        public TimeSpan BalancedPeriodTarget{get;set;}

        public Color DisplayColor{get;set;}

        public bool Legal { get; set; }

        public bool HasDayOff { get; set; }

        public bool HasShift { get; set; }

        public string DisplayName { get; set; }

        public string DisplayShortName { get; set; }

        public bool HasAbsence { get; set; }

        public bool HasPersonalAssignmentOnly { get; set; }

        public string TipText { get; set; }

        public bool HasPreference { get; set; }
    }
}