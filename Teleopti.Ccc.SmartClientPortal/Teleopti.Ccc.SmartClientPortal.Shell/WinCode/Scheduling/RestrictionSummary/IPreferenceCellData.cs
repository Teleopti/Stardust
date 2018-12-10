using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary
{
    public interface IPreferenceCellData 
    {
        DateOnly TheDate { get; set; }
        bool Enabled { get; set; }
        bool Legal { get; set; }
        TimeSpan WeeklyMax { get; set; }
        TimeSpan WeeklyMin { get; set; }
        TimeSpan PeriodTarget { get; set; }
        bool IsInsidePeriod { get; set; }
        Color DisplayColor { get; set; }
        string DisplayName { get; set; }
        string DisplayShortName { get; set; }
        bool HasShift { get; set; }
        bool HasDayOff { get; set; }
        bool HasAbsence { get; set; }
        bool HasFullDayAbsence { get; set; }
        bool HasActivityPreference { get; set; }
        RestrictionSchedulingOptions SchedulingOption { get; set; }
        IScheduleDay SchedulePart { get; set; }
        IEffectiveRestriction EffectiveRestriction { get; set; }
        bool HasExtendedPreference { get; set; }
        bool MustHavePreference { get; set; }
        string ShiftLengthScheduledShift { get; set; }
        string StartEndScheduledShift { get; set; }
        bool HasAbsenceOnContractDayOff { get; set; }
    	bool ViolatesNightlyRest { get; set; }
        bool NoShiftsCanBeFound { get; set; }
    	TimeSpan NightlyRest { get; set; }
        EmploymentType EmploymentType { get; set; }
    }
}
