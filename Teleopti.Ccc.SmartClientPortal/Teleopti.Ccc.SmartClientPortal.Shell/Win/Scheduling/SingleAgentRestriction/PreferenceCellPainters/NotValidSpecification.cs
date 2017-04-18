using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters
{
    public class NotValidSpecification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool IsSatisfiedBy(IPreferenceCellData cellData)
        {
            if((cellData.HasShift || cellData.HasFullDayAbsence || cellData.HasDayOff) && cellData.SchedulingOption.UseScheduling)
                return false;

            if (cellData.ViolatesNightlyRest)
                return true;

            if (cellData.NoShiftsCanBeFound)
                return true;

            if ((cellData.SchedulingOption.UsePreferences && cellData.SchedulingOption.UseRotations) && cellData.EffectiveRestriction != null
                && (cellData.EffectiveRestriction.DayOffTemplate != null && cellData.EffectiveRestriction.ShiftCategory != null))
                return true;
            if (((!cellData.HasShift || !cellData.HasFullDayAbsence || !cellData.HasDayOff) && cellData.SchedulingOption.UseScheduling) 
                && ((cellData.EffectiveRestriction != null && cellData.EffectiveRestriction.DayOffTemplate != null && cellData.EffectiveRestriction.ShiftCategory != null)
                || cellData.EffectiveRestriction == null))
                return true;
            return (((cellData.SchedulingOption.UsePreferences || cellData.SchedulingOption.UseRotations || cellData.SchedulingOption.UseAvailability) && !cellData.SchedulingOption.UseScheduling) && cellData.EffectiveRestriction == null);
        }
    }
}