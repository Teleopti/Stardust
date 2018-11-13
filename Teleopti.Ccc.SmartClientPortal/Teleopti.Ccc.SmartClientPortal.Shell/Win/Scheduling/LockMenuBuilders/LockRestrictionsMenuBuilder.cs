using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.LockMenuBuilders
{
	public class LockRestrictionsMenuBuilder
	{
		public void BuildLockAvailability(ToolStripMenuItem toolStripMenuItemLockAvailabilityRM, ToolStripMenuItem toolStripMenuItemLockAvailability, UserLockHelper userLockHelper)
		{
			//AllUnavailable
			var toolStripMenuItemAllUnavailableAvailability = new ToolStripMenuItem(Resources.AllUnavailable)
				{ Name = "toolStripMenuItemAllUnavailableAvailability" };
			toolStripMenuItemAllUnavailableAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailability.DropDownItems.Add(toolStripMenuItemAllUnavailableAvailability);

			var toolStripMenuItemAllUnAvailableAvailabilityRM = new ToolStripMenuItem(Resources.AllUnavailable)
				{ Name = "toolStripMenuItemAllUnAvailableAvailabilityRM" };
			toolStripMenuItemAllUnAvailableAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllUnAvailableAvailabilityRM);

			//AllAvailable
			var toolStripMenuItemAllAvailableAvailability = new ToolStripMenuItem(Resources.AllAvailable)
				{ Name = "toolStripMenuItemAllAvailableAvailability" };
			toolStripMenuItemAllAvailableAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllAvailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailability.DropDownItems.Add(toolStripMenuItemAllAvailableAvailability);

			var toolStripMenuItemAllAvailableAvailabilityRM = new ToolStripMenuItem(Resources.AllAvailable)
				{ Name = "toolStripMenuItemAllAvailableAvailabilityRM" };
			toolStripMenuItemAllAvailableAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllAvailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllAvailableAvailabilityRM);

			//AllFulFilled
			var toolStripMenuItemAllFulFilledAvailability = new ToolStripMenuItem(Resources.AllFulFilled)
				{Name = "toolStripMenuItemAllFulFilledAvailability"};
			toolStripMenuItemAllFulFilledAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledAvailabilityMouseUp;
			toolStripMenuItemLockAvailability.DropDownItems.Add(toolStripMenuItemAllFulFilledAvailability);

			var toolStripMenuItemAllFulFilledAvailabilityRM = new ToolStripMenuItem(Resources.AllFulFilled)
				{ Name = "toolStripMenuItemAllFulFilledAvailabilityRM" };
			toolStripMenuItemAllFulFilledAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledAvailabilityMouseUp;
			toolStripMenuItemLockAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllFulFilledAvailabilityRM);
		}

		public void BuildLockStudentAvailability(ToolStripMenuItem toolStripMenuItemLockStudentAvailability, ToolStripMenuItem toolStripMenuItemLockStudentAvailabilityRM, UserLockHelper userLockHelper)
		{
			//AllUnavailable
			var toolStripMenuItemAllUnavailableStudentAvailability = new ToolStripMenuItem(Resources.AllUnavailable)
			{ Name = "toolStripMenuItemAllUnavailableStudentAvailability" };
			toolStripMenuItemAllUnavailableStudentAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailability.DropDownItems.Add(toolStripMenuItemAllUnavailableStudentAvailability);

			var toolStripMenuItemAllUnavailableStudentAvailabilityRM = new ToolStripMenuItem(Resources.AllUnavailable)
			{ Name = "toolStripMenuItemAllUnavailableStudentAvailabilityRM" };
			toolStripMenuItemAllUnavailableStudentAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllUnavailableStudentAvailabilityRM);

			//AllAvailable
			var toolStripMenuItemAllAvailableStudentAvailability = new ToolStripMenuItem(Resources.AllAvailable)
			{ Name = "toolStripMenuItemAllAvailableStudentAvailability" };
			toolStripMenuItemAllAvailableStudentAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllAvailableStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailability.DropDownItems.Add(toolStripMenuItemAllAvailableStudentAvailability);

			var toolStripMenuItemAllAvailableStudentAvailabilityRM = new ToolStripMenuItem(Resources.AllAvailable)
			{ Name = "toolStripMenuItemAllAvailableStudentAvailabilityRM" };
			toolStripMenuItemAllAvailableStudentAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllAvailableStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllAvailableStudentAvailabilityRM);

			//AllFulFilled
			var toolStripMenuItemAllFulFilledStudentAvailability = new ToolStripMenuItem(Resources.AllFulFilled)
			{ Name = "toolStripMenuItemAllFulFilledStudentAvailability" };
			toolStripMenuItemAllFulFilledStudentAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailability.DropDownItems.Add(toolStripMenuItemAllFulFilledStudentAvailability);

			var toolStripMenuItemAllFulFilledStudentAvailabilityRM = new ToolStripMenuItem(Resources.AllFulFilled)
			{ Name = "toolStripMenuItemAllFulFilledStudentAvailabilityRM" };
			toolStripMenuItemAllFulFilledStudentAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllFulFilledStudentAvailabilityRM);
		}

		public void BuildLockRotation(ToolStripMenuItem toolStripMenuItemLockRotations, ToolStripMenuItem toolStripMenuItemLockRotationsRM, UserLockHelper userLockHelper)
		{
			//All, toolStripMenuItemAllRotations, toolStripMenuItemAllRotationsRM, toolStripMenuItemAllRotationsMouseUp
			var toolStripMenuItemAllRotations = new ToolStripMenuItem(Resources.All)
				{ Name = "toolStripMenuItemAllRotations" };
			toolStripMenuItemAllRotations.MouseUp += userLockHelper.ToolStripMenuItemAllRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllRotations);

			var toolStripMenuItemAllRotationsRM = new ToolStripMenuItem(Resources.All)
				{ Name = "toolStripMenuItemAllRotationsRM" };
			toolStripMenuItemAllRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllRotationsRM);

			//AllDaysOff, toolStripMenuItemAllDaysOffRotations, toolStripMenuItemAllDaysOffRotationsRM, toolStripMenuItemAllDaysOffRotationsMouseUp
			var toolStripMenuItemAllDaysOffRotations = new ToolStripMenuItem(Resources.AllDaysOff)
				{ Name = "toolStripMenuItemAllDaysOffRotations" };
			toolStripMenuItemAllDaysOffRotations.MouseUp += userLockHelper.ToolStripMenuItemAllDaysOffRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllDaysOffRotations);

			var toolStripMenuItemAllDaysOffRotationsRM = new ToolStripMenuItem(Resources.AllDaysOff)
				{ Name = "toolStripMenuItemAllDaysOffRotationsRM" };
			toolStripMenuItemAllDaysOffRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllDaysOffRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllDaysOffRotationsRM);

			//AllShifts, toolStripMenuItemAllShiftRotations, toolStripMenuItemAllShiftRotationsRM, toolStripMenuItemAllShiftsRotationsMouseUp
			var toolStripMenuItemAllShiftRotations = new ToolStripMenuItem(Resources.AllShifts)
				{ Name = "toolStripMenuItemAllShiftRotations" };
			toolStripMenuItemAllShiftRotations.MouseUp += userLockHelper.ToolStripMenuItemAllShiftsRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllShiftRotations);

			var toolStripMenuItemAllShiftRotationsRM = new ToolStripMenuItem(Resources.AllShifts)
				{ Name = "toolStripMenuItemAllShiftRotationsRM" };
			toolStripMenuItemAllShiftRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllShiftsRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllShiftRotationsRM);

			//AllFulFilled, toolStripMenuItemAllFulFilledRotations, toolStripMenuItemAllFulFilledRotationsRM, toolStripMenuItemAllFulFilledRotationsMouseUp
			var toolStripMenuItemAllFulFilledRotations = new ToolStripMenuItem(Resources.AllFulFilled)
				{ Name = "toolStripMenuItemAllFulFilledRotations" };
			toolStripMenuItemAllFulFilledRotations.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllFulFilledRotations);

			var toolStripMenuItemAllFulFilledRotationsRM = new ToolStripMenuItem(Resources.AllFulFilled)
				{ Name = "toolStripMenuItemAllFulFilledRotationsRM" };
			toolStripMenuItemAllFulFilledRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllFulFilledRotationsRM);

			//AllFulFilledDaysOff, toolStripMenuItemAllFulFilledDaysOffRotations, toolStripMenuItemAllFulFilledDaysOffRotationsRM, toolStripMenuItemAllFulFilledDaysOffRotationsMouseUp
			var toolStripMenuItemAllFulFilledDaysOffRotations = new ToolStripMenuItem(Resources.AllFulFilledDaysOff)
				{ Name = "toolStripMenuItemAllFulFilledDaysOffRotations" };
			toolStripMenuItemAllFulFilledDaysOffRotations.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledDaysOffRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllFulFilledDaysOffRotations);

			var toolStripMenuItemAllFulFilledDaysOffRotationsRM = new ToolStripMenuItem(Resources.AllFulFilledDaysOff)
				{ Name = "toolStripMenuItemAllFulFilledDaysOffRotationsRM" };
			toolStripMenuItemAllFulFilledDaysOffRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledDaysOffRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllFulFilledDaysOffRotationsRM);

			//AllFulFilledShifts, toolStripMenuItemAllFulFilledShiftsRotations, toolStripMenuItemAllFulFilledShiftsRotationsRM, toolStripMenuItemAllFulFilledShiftsRotationsMouseUp
			var toolStripMenuItemAllFulFilledShiftsRotations = new ToolStripMenuItem(Resources.AllFulFilledShifts)
				{ Name = "toolStripMenuItemAllFulFilledShiftsRotations" };
			toolStripMenuItemAllFulFilledShiftsRotations.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledShiftsRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllFulFilledShiftsRotations);

			var toolStripMenuItemAllFulFilledShiftsRotationsRM = new ToolStripMenuItem(Resources.AllFulFilledShifts)
				{ Name = "toolStripMenuItemAllFulFilledShiftsRotationsRM" };
			toolStripMenuItemAllFulFilledShiftsRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledShiftsRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllFulFilledShiftsRotationsRM);
		}

		public void BuildLockPreference(ToolStripMenuItem toolStripMenuItemLockPreferences, ToolStripMenuItem toolStripMenuItemLockPreferencesRM, UserLockHelper userLockHelper)
		{
			//All, toolStripMenuItemAllPreferences, toolStripMenuItemAllPreferencesRM, toolStripMenuItemAllPreferencesMouseUp
			var toolStripMenuItemAllPreferences = new ToolStripMenuItem(Resources.All)
				{ Name = "toolStripMenuItemAllRotations" };
			toolStripMenuItemAllPreferences.MouseUp += userLockHelper.ToolStripMenuItemAllPreferencesMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllPreferences);

			var toolStripMenuItemAllPreferencesRM = new ToolStripMenuItem(Resources.All)
				{ Name = "toolStripMenuItemAllRotationsRM" };
			toolStripMenuItemAllPreferencesRM.MouseUp += userLockHelper.ToolStripMenuItemAllPreferencesMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllPreferencesRM);

			//AllAbsences, toolStripMenuItemAllAbsencePreference, toolStripMenuItemAllAbsencePreferenceRM, toolStripMenuItemAllAbsencePreferenceMouseUp
			var toolStripMenuItemAllAbsencePreference = new ToolStripMenuItem(Resources.AllAbsences)
				{ Name = "toolStripMenuItemAllAbsencePreference" };
			toolStripMenuItemAllAbsencePreference.MouseUp += userLockHelper.ToolStripMenuItemAllAbsencePreferenceMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllAbsencePreference);

			var toolStripMenuItemAllAbsencePreferenceRM = new ToolStripMenuItem(Resources.AllAbsences)
				{ Name = "toolStripMenuItemAllAbsencePreferenceRM" };
			toolStripMenuItemAllAbsencePreferenceRM.MouseUp += userLockHelper.ToolStripMenuItemAllAbsencePreferenceMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllAbsencePreferenceRM);

			//AllDaysOff, toolStripMenuItemAllDaysOff, toolStripMenuItemAllDaysOffRM, toolStripMenuItemAllDaysOffMouseUp
			var toolStripMenuItemAllDaysOff = new ToolStripMenuItem(Resources.AllDaysOff)
				{ Name = "toolStripMenuItemAllDaysOff" };
			toolStripMenuItemAllDaysOff.MouseUp += userLockHelper.ToolStripMenuItemAllDaysOffMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllDaysOff);

			var toolStripMenuItemAllDaysOffRM = new ToolStripMenuItem(Resources.AllDaysOff)
				{ Name = "toolStripMenuItemAllDaysOffRM" };
			toolStripMenuItemAllDaysOffRM.MouseUp += userLockHelper.ToolStripMenuItemAllDaysOffMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllDaysOffRM);

			//AllShifts, toolStripMenuItemAllShiftsPreferences, toolStripMenuItemAllShiftsPreferencesRM, toolStripMenuItemAllShiftsPreferencesMouseUp
			var toolStripMenuItemAllShiftsPreferences = new ToolStripMenuItem(Resources.AllShifts)
				{ Name = "toolStripMenuItemAllShiftsPreferences" };
			toolStripMenuItemAllShiftsPreferences.MouseUp += userLockHelper.ToolStripMenuItemAllShiftsPreferencesMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllShiftsPreferences);

			var toolStripMenuItemAllShiftsPreferencesRM = new ToolStripMenuItem(Resources.AllShifts)
				{ Name = "toolStripMenuItemAllShiftsPreferencesRM" };
			toolStripMenuItemAllShiftsPreferencesRM.MouseUp += userLockHelper.ToolStripMenuItemAllShiftsPreferencesMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllShiftsPreferencesRM);

			//AllMustHave, toolStripMenuItemAllMustHave, toolStripMenuItemAllMustHaveRM, toolStripMenuItemAllMustHaveMouseUp
			var toolStripMenuItemAllMustHave = new ToolStripMenuItem(Resources.AllMustHave)
				{ Name = "toolStripMenuItemAllMustHave" };
			toolStripMenuItemAllMustHave.MouseUp += userLockHelper.ToolStripMenuItemAllMustHaveMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllMustHave);

			var toolStripMenuItemAllMustHaveRM = new ToolStripMenuItem(Resources.AllMustHave)
				{ Name = "toolStripMenuItemAllMustHaveRM" };
			toolStripMenuItemAllMustHaveRM.MouseUp += userLockHelper.ToolStripMenuItemAllMustHaveMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllMustHaveRM);

			//AllFulFilled, toolStripMenuItemAllFulFilledPreferences, toolStripMenuItemAllFulFilledPreferencesRM, toolStripMenuItemAllFulFilledPreferencesMouseUp
			var toolStripMenuItemAllFulFilledPreferences = new ToolStripMenuItem(Resources.AllFulFilled)
				{ Name = "toolStripMenuItemAllFulFilledPreferences" };
			toolStripMenuItemAllFulFilledPreferences.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledPreferencesMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllFulFilledPreferences);

			var toolStripMenuItemAllFulFilledPreferencesRM = new ToolStripMenuItem(Resources.AllFulFilled)
				{ Name = "toolStripMenuItemAllFulFilledPreferencesRM" };
			toolStripMenuItemAllFulFilledPreferencesRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledPreferencesMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllFulFilledPreferencesRM);

			//AllFulFilledAbsences, toolStripMenuItemAllFulFilledAbsencesPreferences, toolStripMenuItemAllFulFilledAbsencesPreferencesRM, toolStripMenuItemAllAbsencePreferenceMouseUp
			var toolStripMenuItemAllFulFilledAbsencesPreferences = new ToolStripMenuItem(Resources.AllFulFilledAbsences)
				{ Name = "toolStripMenuItemAllFulFilledAbsencesPreferences" };
			toolStripMenuItemAllFulFilledAbsencesPreferences.MouseUp += userLockHelper.ToolStripMenuItemAllAbsencePreferenceMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllFulFilledAbsencesPreferences);

			var toolStripMenuItemAllFulFilledAbsencesPreferencesRM = new ToolStripMenuItem(Resources.AllFulFilledAbsences)
				{ Name = "toolStripMenuItemAllFulFilledAbsencesPreferencesRM" };
			toolStripMenuItemAllFulFilledAbsencesPreferencesRM.MouseUp += userLockHelper.ToolStripMenuItemAllAbsencePreferenceMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllFulFilledAbsencesPreferencesRM);

			//AllFulFilledDaysOff, toolStripMenuItemAllFulFilledDaysOffPreferences, toolStripMenuItemAllFulFilledDaysOffPreferencesRM, toolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp
			var toolStripMenuItemAllFulFilledDaysOffPreferences = new ToolStripMenuItem(Resources.AllFulFilledDaysOff)
				{ Name = "toolStripMenuItemAllFulFilledDaysOffPreferences" };
			toolStripMenuItemAllFulFilledDaysOffPreferences.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllFulFilledDaysOffPreferences);

			var toolStripMenuItemAllFulFilledDaysOffPreferencesRM = new ToolStripMenuItem(Resources.AllFulFilledDaysOff)
				{ Name = "toolStripMenuItemAllFulFilledDaysOffPreferencesRM" };
			toolStripMenuItemAllFulFilledDaysOffPreferencesRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllFulFilledDaysOffPreferencesRM);

			//AllFulFilledShifts, toolStripMenuItemAllFulFilledShiftsPreferences, toolStripMenuItemAllFulFilledShiftsPreferencesRM, toolStripMenuItemAllFulFilledShiftsPreferencesMouseUp
			var toolStripMenuItemAllFulFilledShiftsPreferences = new ToolStripMenuItem(Resources.AllFulFilledShifts)
				{ Name = "toolStripMenuItemAllFulFilledShiftsPreferences" };
			toolStripMenuItemAllFulFilledShiftsPreferences.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledShiftsPreferencesMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllFulFilledShiftsPreferences);

			var toolStripMenuItemAllFulFilledShiftsPreferencesRM = new ToolStripMenuItem(Resources.AllFulFilledShifts)
				{ Name = "toolStripMenuItemAllFulFilledShiftsPreferencesRM" };
			toolStripMenuItemAllFulFilledShiftsPreferencesRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledShiftsPreferencesMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllFulFilledShiftsPreferencesRM);

			//AllFulFilledMustHave, toolStripMenuItemAllFulfilledMustHave, toolStripMenuItemAllFulfilledMustHaveRM, toolStripMenuItemAllFulfilledMustHaveMouseUp
			var toolStripMenuItemAllFulfilledMustHave = new ToolStripMenuItem(Resources.AllFulfilledMustHave)
				{ Name = "toolStripMenuItemAllFulfilledMustHave" };
			toolStripMenuItemAllFulfilledMustHave.MouseUp += userLockHelper.ToolStripMenuItemAllFulfilledMustHaveMouseUp;
			toolStripMenuItemLockPreferences.DropDownItems.Add(toolStripMenuItemAllFulfilledMustHave);

			var toolStripMenuItemAllFulfilledMustHaveRM = new ToolStripMenuItem(Resources.AllFulfilledMustHave)
				{ Name = "toolStripMenuItemAllFulfilledMustHaveRM" };
			toolStripMenuItemAllFulfilledMustHaveRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulfilledMustHaveMouseUp;
			toolStripMenuItemLockPreferencesRM.DropDownItems.Add(toolStripMenuItemAllFulfilledMustHaveRM);
		}
	}
}