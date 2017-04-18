using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface IAgentPreferenceView
	{
		void PopulateShiftCategories();
		void PopulateAbsences();
		void PopulateDayOffs();
		void PopulateActivities();
		void ClearShiftCategory();
		void ClearShiftCategoryExtended();
		void ClearAbsence();
		void ClearDayOff();
		void ClearActivity();
		void UpdateShiftCategory(IShiftCategory shiftCategory);
		//void UpdateShiftCategoryExtended(IShiftCategory shiftCategory);
		void UpdateAbsence(IAbsence absence);
		void UpdateDayOff(IDayOffTemplate dayOffTemplate);
		void UpdateActivity(IActivity activity);
		void UpdateActivityTimes(TimeSpan? minLength, TimeSpan? maxLength, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd);
		void UpdateTimesExtended(TimeSpan? minLength, TimeSpan? maxLength, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd);
		void UpdateMustHave(bool mustHave);
		void UpdateMustHaveText(string text);
	}
}
