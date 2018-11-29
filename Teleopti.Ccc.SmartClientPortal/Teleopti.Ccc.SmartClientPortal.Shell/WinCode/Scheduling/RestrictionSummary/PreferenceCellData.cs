using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary
{
    public class PreferenceCellData : IPreferenceCellData
    {
        //private Limitation.Preference _preference;

    	public PreferenceCellData()
        {
            Enabled = true;
        }

        public DateOnly TheDate { get; set; }

        public bool Enabled { get; set; }
        public bool Legal { get; set; }
        public TimeSpan WeeklyMax { get; set; }
        public TimeSpan WeeklyMin { get; set; }
        public TimeSpan PeriodTarget { get; set; }
        public bool IsInsidePeriod { get; set; }
        public Color DisplayColor { get; set; }
        public string DisplayName { get; set; }
        public string DisplayShortName { get; set; }
        public bool HasShift { get; set; }
        public bool HasDayOff { get; set; }
        public bool HasAbsence { get; set; }
        public bool HasFullDayAbsence { get; set; }
        public bool HasActivityPreference { get; set; }
        public bool HasAbsenceOnContractDayOff { get; set; }
        public RestrictionSchedulingOptions SchedulingOption { get; set; }
        public IScheduleDay SchedulePart { get; set; }
        public bool HasExtendedPreference { get; set; }
        public bool MustHavePreference { get; set; }
        public string ShiftLengthScheduledShift { get; set; }
        public string StartEndScheduledShift { get; set; }
    	public IEffectiveRestriction EffectiveRestriction { get; set; }
		public bool ViolatesNightlyRest { get; set; }
        public bool NoShiftsCanBeFound { get; set; }
		public TimeSpan NightlyRest { get; set; }
        public EmploymentType EmploymentType { get; set; }
    }
}