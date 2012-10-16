using System;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsPreferencePainterDecider
	{
		private readonly bool _shouldPaint;

		public AgentRestrictionsPreferencePainterDecider()
		{
			_shouldPaint = true;
		}
		
		public bool ShouldPaint(IPreferenceCellData preferenceCellData)
		{
			if(preferenceCellData == null) throw new ArgumentNullException("preferenceCellData");

			if (preferenceCellData.SchedulingOption.UseScheduling && (preferenceCellData.HasFullDayAbsence || preferenceCellData.HasShift || preferenceCellData.HasDayOff)) return false;
			if (!preferenceCellData.SchedulingOption.UsePreferences && !preferenceCellData.SchedulingOption.UseRotations && !preferenceCellData.SchedulingOption.UseAvailability && !preferenceCellData.SchedulingOption.UseStudentAvailability) return false;

			return _shouldPaint;
		}

		public bool ShouldPaintPreferredDayOff(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData == null) throw new ArgumentNullException("preferenceCellData");

			if (preferenceCellData.EffectiveRestriction == null) return false;	
			if (preferenceCellData.EffectiveRestriction.DayOffTemplate == null) return false;
			if ((preferenceCellData.SchedulingOption.UsePreferences && preferenceCellData.SchedulingOption.UseRotations) && preferenceCellData.EffectiveRestriction.ShiftCategory != null) return false;

			return _shouldPaint;
		}

		public bool ShouldPaintPreferredShiftCategory(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData == null) throw new ArgumentNullException("preferenceCellData");

			if (preferenceCellData.EffectiveRestriction == null) return false;	
			if (preferenceCellData.EffectiveRestriction.ShiftCategory == null) return false;
			if ((preferenceCellData.SchedulingOption.UsePreferences && preferenceCellData.SchedulingOption.UseRotations) && preferenceCellData.EffectiveRestriction.DayOffTemplate != null) return false;

			return _shouldPaint;
		}

		public bool ShouldPaintPreferredAbsence(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData == null) throw new ArgumentNullException("preferenceCellData");

			if (preferenceCellData.EffectiveRestriction == null) return false;
			if (preferenceCellData.EffectiveRestriction.Absence == null) return false;
			if (preferenceCellData.HasAbsenceOnContractDayOff) return false;
			if ((preferenceCellData.SchedulingOption.UsePreferences && preferenceCellData.SchedulingOption.UseRotations) && preferenceCellData.EffectiveRestriction.DayOffTemplate != null) return false;

			return _shouldPaint;
		}

		public bool ShouldPaintPreferredAbsenceOnContractDayOff(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData == null) throw new ArgumentNullException("preferenceCellData");

			if (preferenceCellData.EffectiveRestriction == null) return false;
			if (preferenceCellData.EffectiveRestriction.Absence == null) return false;
			if (!preferenceCellData.HasAbsenceOnContractDayOff) return false;
			if ((preferenceCellData.SchedulingOption.UsePreferences && preferenceCellData.SchedulingOption.UseRotations) && preferenceCellData.EffectiveRestriction.DayOffTemplate != null) return false;

			return _shouldPaint;
		}

		public bool ShouldPaintPreferredExtended(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData == null) throw new ArgumentNullException("preferenceCellData");

			if (!preferenceCellData.SchedulingOption.UsePreferences) return false;
			if (!preferenceCellData.HasExtendedPreference) return false;
			if (preferenceCellData.SchedulingOption.UseRotations && preferenceCellData.EffectiveRestriction.DayOffTemplate != null) return false;

			return _shouldPaint;
		}
	}
}
