using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.GridlockCommands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class LockExecuter
	{
		private readonly GridControl _grid;
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IGridlockManager _lockManager;
		private readonly SchedulingScreen _schedulingScreen;

		public LockExecuter(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager, SchedulingScreen schedulingScreen)
		{
			_grid = grid;
			_schedulerState = schedulerState;
			_lockManager = lockManager;
			_schedulingScreen = schedulingScreen;
		}

		public void LockAllRestrictions(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayRestrictionExtractor scheduleDayRestrictionExtractor = new ScheduleDayRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllMustHaveFulfilledCommand = new GridlockAllPreferencesMustHaveFulfilledCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllMustHaveFulfilledCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledPreferences(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllPreferencesFulfilledCommand = new GridlockAllPreferencesFulfilledCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesFulfilledCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllAbsencePreference(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
			var gridlockAllPreferencesAbsenceCommand = new GridlockAllPreferencesAbsenceCommand(gridSchedulesExtractor, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesAbsenceCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledAbsencesPreferences(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllPreferencesFulfilledAbsenceCommand = new GridlockAllPreferencesFulfilledAbsenceCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayPreferenceRestrictionExtractor, _lockManager);
			gridlockAllPreferencesFulfilledAbsenceCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}

		public void AllFulfilledDaysOffPreferences(MouseButtons mouseButtons)
		{
			if (mouseButtons != MouseButtons.Left) return;
			_schedulingScreen.Cursor = Cursors.WaitCursor;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllPreferencesFulfilledDaysOffCommand = new GridlockAllPreferencesFulfilledDayOffCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayPreferenceRestrictionExtractor, _lockManager);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor = new ScheduleDayPreferenceRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationExtractor = new ScheduleDayRotationRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationExtractor = new ScheduleDayRotationRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationExtractor = new ScheduleDayRotationRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationRestrictionExtractor = new ScheduleDayRotationRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationRestrictionExtractor = new ScheduleDayRotationRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayRotationRestrictionExtractor scheduleDayRotationRestrictionExtractor = new ScheduleDayRotationRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityExtractor = new ScheduleDayStudentAvailabilityRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityExtractor = new ScheduleDayStudentAvailabilityRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityRestrictionExtractor = new ScheduleDayStudentAvailabilityRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityExtractor = new ScheduleDayAvailabilityRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityExtractor = new ScheduleDayAvailabilityRestrictionExtractor(restrictionExtractor);
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
			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_schedulerState.SchedulingResultState);
			IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityRestrictionExtractor = new ScheduleDayAvailabilityRestrictionExtractor(restrictionExtractor);
			ICheckerRestriction restrictionChecker = new RestrictionChecker();
			var gridlockAllAvailabilityFulfilledCommand = new GridlockAllAvailabilityFulfilledCommand(gridSchedulesExtractor, restrictionChecker, scheduleDayAvailabilityRestrictionExtractor, _lockManager);
			gridlockAllAvailabilityFulfilledCommand.Execute();
			_schedulingScreen.Refresh();
			_schedulingScreen.RefreshSelection();
			_schedulingScreen.Cursor = Cursors.Default;
		}
	}
}
