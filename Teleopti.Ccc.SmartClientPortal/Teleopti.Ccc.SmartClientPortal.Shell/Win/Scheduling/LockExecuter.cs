using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class LockExecuter
	{
		private readonly GridControl _grid;
		private readonly IGridlockManager _lockManager;
		private readonly IRestrictionExtractor _restrictionExtractor;
		private readonly SchedulingScreen _schedulingScreen;

		public LockExecuter(GridControl grid, IRestrictionExtractor restrictionExtractor, IGridlockManager lockManager, SchedulingScreen schedulingScreen)
		{
			_grid = grid;
			_lockManager = lockManager;
			_restrictionExtractor = restrictionExtractor;
			_schedulingScreen = schedulingScreen;
		}

		public void LockAllRestrictions(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayRestrictionExtractor scheduleDayRestrictionExtractor = new ScheduleDayRestrictionExtractor(_restrictionExtractor);
			var gridlockAllRestrictionsCommand = new GridlockAllRestrictionsCommand(gridSchedulesExtractor, scheduleDayRestrictionExtractor, _lockManager);
			gridlockAllRestrictionsCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllPreferences(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			var gridlockAllPreferencesCommand = new GridlockAllPreferencesCommand(gridSchedulesExtractor, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllDaysOff(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			var gridlockAllPreferencesDayOffCommand = new GridlockAllPreferencesDayOffCommand(gridSchedulesExtractor, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesDayOffCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllShiftsPreferences(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			var gridlockAllPreferencesShiftCommand = new GridlockAllPreferencesShiftCommand(gridSchedulesExtractor, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesShiftCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllMustHave(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			var gridlockAllPreferencesMustHaveCommand = new GridlockAllPreferencesMustHaveCommand(gridSchedulesExtractor, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesMustHaveCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledMustHave(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllMustHaveFulfilledCommand = new GridlockAllPreferencesMustHaveFulfilledCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllMustHaveFulfilledCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllAbsencePreference(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			var gridlockAllPreferencesAbsenceCommand = new GridlockAllPreferencesAbsenceCommand(gridSchedulesExtractor, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesAbsenceCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledDaysOffPreferences(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllPreferencesFulfilledDaysOffCommand = new GridlockAllPreferencesFulfilledDayOffCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesFulfilledDaysOffCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledAbsencePreferences(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllPreferencesFulfilledDaysOffCommand = new GridlockAllPreferencesFulfilledAbsenceCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesFulfilledDaysOffCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledShiftsPreferences(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllPreferencesFulfilledShiftCommand = new GridlockAllPreferencesFulfilledShiftCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesFulfilledShiftCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllRotations(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationExtractor = new ScheduleDayRotationRestrictionExtractor(_restrictionExtractor);
			var gridlockAllRotationsCommand = new GridlockAllRotationsCommand(gridSchedulesExtractor, scheduleDayRotationExtractor, _lockManager);
			gridlockAllRotationsCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllDaysOffRotations(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationExtractor = new ScheduleDayRotationRestrictionExtractor(_restrictionExtractor);
			var gridlockAllRotationsDayOffCommand = new GridlockAllRotationsDayOffCommand(gridSchedulesExtractor, scheduleDayRotationExtractor, _lockManager);
			gridlockAllRotationsDayOffCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllShiftsRotations(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationExtractor = new ScheduleDayRotationRestrictionExtractor(_restrictionExtractor);
			var gridlockAllRotationsShiftCommand = new GridlockAllRotationsShiftCommand(gridSchedulesExtractor, scheduleDayRotationExtractor, _lockManager);
			gridlockAllRotationsShiftCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledRotations(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationRestrictionExtractor = new ScheduleDayRotationRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllRotationsFulfilledCommand = new GridlockAllRotationsFulfilledCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayRotationRestrictionExtractor, _lockManager);
			gridlockAllRotationsFulfilledCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledDaysOffRotations(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationRestrictionExtractor = new ScheduleDayRotationRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllRotationsFulfilledDayOffCommand = new GridlockAllRotationsFulfilledDayOffCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayRotationRestrictionExtractor, _lockManager);
			gridlockAllRotationsFulfilledDayOffCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledShiftsRotations(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationRestrictionExtractor = new ScheduleDayRotationRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllRotationsFulfilledShiftCommand = new GridlockAllRotationsFulfilledShiftCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayRotationRestrictionExtractor, _lockManager);
			gridlockAllRotationsFulfilledShiftCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllUnavailableStudentAvailability(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityExtractor = new ScheduleDayStudentAvailabilityRestrictionExtractor(_restrictionExtractor);
			var gridlockAllStudentAvailabilityUnavailableCommand = new GridlockAllStudentAvailabilityUnavailableCommand(gridSchedulesExtractor, scheduleDayStudentAvailabilityExtractor, _lockManager);
			gridlockAllStudentAvailabilityUnavailableCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllAvailableStudentAvailability(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityExtractor = new ScheduleDayStudentAvailabilityRestrictionExtractor(_restrictionExtractor);
			var gridlockAllStudentAvailabilityAvailableCommand = new GridlockAllStudentAvailabilityAvailableCommand(gridSchedulesExtractor, scheduleDayStudentAvailabilityExtractor, _lockManager);
			gridlockAllStudentAvailabilityAvailableCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledStudentAvailability(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityRestrictionExtractor = new ScheduleDayStudentAvailabilityRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllStudentAvailabilityFulfilledCommand = new GridlockAllStudentAvailabilityFulfilledCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayStudentAvailabilityRestrictionExtractor, _lockManager);
			gridlockAllStudentAvailabilityFulfilledCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllUnavailableAvailability(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityExtractor = new ScheduleDayAvailabilityRestrictionExtractor(_restrictionExtractor);
			var gridlockAllAvailabilityUnavailableCommand = new GridlockAllAvailabilityUnavailableCommand(gridSchedulesExtractor, scheduleDayAvailabilityExtractor, _lockManager);
			gridlockAllAvailabilityUnavailableCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllAvailableAvailability(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityExtractor = new ScheduleDayAvailabilityRestrictionExtractor(_restrictionExtractor);
			var gridlockAllAvailabilityAvailableCommand = new GridlockAllAvailabilityAvailableCommand(gridSchedulesExtractor, scheduleDayAvailabilityExtractor, _lockManager);
			gridlockAllAvailabilityAvailableCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledAvailability(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityRestrictionExtractor = new ScheduleDayAvailabilityRestrictionExtractor(_restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllAvailabilityFulfilledCommand = new GridlockAllAvailabilityFulfilledCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayAvailabilityRestrictionExtractor, _lockManager);
			gridlockAllAvailabilityFulfilledCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}
	}
}
