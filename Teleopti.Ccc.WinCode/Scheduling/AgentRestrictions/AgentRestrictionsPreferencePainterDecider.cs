using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsPreferencePainterDecider
	{

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool ShouldPaint(IPreferenceCellData preferenceCellData)
		{
			if (!preferenceCellData.Enabled) return false;
			if (preferenceCellData.SchedulingOption.UseScheduling && (preferenceCellData.HasFullDayAbsence || preferenceCellData.HasShift || preferenceCellData.HasDayOff)) return false;
			if (!preferenceCellData.SchedulingOption.UsePreferences && !preferenceCellData.SchedulingOption.UseRotations && !preferenceCellData.SchedulingOption.UseAvailability && !preferenceCellData.SchedulingOption.UseStudentAvailability) return false;
		
			
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintPreferredDayOff(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData.EffectiveRestriction == null) return false;	
			if (preferenceCellData.EffectiveRestriction.DayOffTemplate == null) return false;
			if ((preferenceCellData.SchedulingOption.UsePreferences && preferenceCellData.SchedulingOption.UseRotations) && preferenceCellData.EffectiveRestriction.ShiftCategory != null) return false;

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintPreferredShiftCategory(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData.EffectiveRestriction == null) return false;	
			if (preferenceCellData.EffectiveRestriction.ShiftCategory == null) return false;
			if ((preferenceCellData.SchedulingOption.UsePreferences && preferenceCellData.SchedulingOption.UseRotations) && preferenceCellData.EffectiveRestriction.DayOffTemplate != null) return false;

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintPreferredAbsence(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData.EffectiveRestriction == null) return false;
			if (preferenceCellData.EffectiveRestriction.Absence == null) return false;
			if (preferenceCellData.HasAbsenceOnContractDayOff) return false;
			if ((preferenceCellData.SchedulingOption.UsePreferences && preferenceCellData.SchedulingOption.UseRotations) && preferenceCellData.EffectiveRestriction.DayOffTemplate != null) return false;

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintPreferredAbsenceOnContractDayOff(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData.EffectiveRestriction == null) return false;
			if (preferenceCellData.EffectiveRestriction.Absence == null) return false;
			if (!preferenceCellData.HasAbsenceOnContractDayOff) return false;
			if ((preferenceCellData.SchedulingOption.UsePreferences && preferenceCellData.SchedulingOption.UseRotations) && preferenceCellData.EffectiveRestriction.DayOffTemplate != null) return false;

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintPreferredExtended(IPreferenceCellData preferenceCellData)
		{
			if (!preferenceCellData.SchedulingOption.UsePreferences) return false;
			if (!preferenceCellData.HasExtendedPreference) return false;
			if (preferenceCellData.SchedulingOption.UseScheduling) return false;

			return true;
		}
	}
}
