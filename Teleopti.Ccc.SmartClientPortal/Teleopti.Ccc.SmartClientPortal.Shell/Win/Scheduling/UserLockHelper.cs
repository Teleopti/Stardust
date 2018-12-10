using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class UserLockHelper
	{
		private readonly SchedulingScreen _parent;
		private readonly GridControl _grid;
		private readonly IRestrictionExtractor _restrictionExtractor;

		public UserLockHelper(SchedulingScreen parent, GridControl grid, IRestrictionExtractor restrictionExtractor)
		{
			_parent = parent;
			_grid = grid;
			_restrictionExtractor = restrictionExtractor;
		}

		internal void ToolStripMenuItemLockShiftCategoryDaysClick(object sender, EventArgs e)
		{
			lockAllShiftCategories();
		}

		internal void ToolStripMenuItemLockShiftCategoriesClick(object sender, EventArgs e)
		{
			lockShiftCategory(sender);
		}

		internal void ToolStripMenuItemLockShiftCategoriesMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockShiftCategory(sender);
		}

		internal void ToolStripMenuItemLockShiftCategoryDaysMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockAllShiftCategories();
		}

		internal void ToolStripMenuItemLockFreeDaysClick(object sender, EventArgs e)
		{
			lockAllDaysOff();
		}

		internal void ToolStripMenuItemDayOffLockRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockAllDaysOff();
		}

		private void lockAllDaysOff()
		{
			_parent.Cursor = Cursors.WaitCursor;
			GridHelper.GridlockFreeDays(_grid, _parent.LockManager);
			_parent.Refresh();
			_parent.RefreshSelection();
			_parent.Cursor = Cursors.Default;
		}

		internal void ToolStripMenuItemLockSpecificDayOffClick(object sender, EventArgs e)
		{
			_parent.Cursor = Cursors.WaitCursor;
			var dayOffTemplate = (IDayOffTemplate)((ToolStripMenuItem)sender).Tag;
			GridHelper.GridlockSpecificDayOff(_grid, _parent.LockManager, dayOffTemplate);
			_parent.Refresh();
			_parent.RefreshSelection();
			_parent.Cursor = Cursors.Default;
		}

		private void lockAllShiftCategories()
		{
			_parent.Cursor = Cursors.WaitCursor;
			GridHelper.GridlockAllShiftCategories(_grid, _parent.LockManager);
			_parent.Refresh();
			_parent.RefreshSelection();
			_parent.Cursor = Cursors.Default;
		}

		private void lockShiftCategory(object sender)
		{
			_parent.Cursor = Cursors.WaitCursor;
			var shiftCategory = (ShiftCategory)((ToolStripMenuItem)sender).Tag;
			GridHelper.GridlockShiftCategories(_grid, _parent.LockManager, shiftCategory);
			_parent.Refresh();
			_parent.RefreshSelection();
			_parent.Cursor = Cursors.Default;
		}


		internal void ToolStripMenuItemLockAbsenceDaysClick(object sender, EventArgs e)
		{
			lockAllAbsences();
		}

		internal void ToolStripMenuItemLockAbsenceDaysMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockAllAbsences();
		}

		private void lockAllAbsences()
		{
			_parent.Cursor = Cursors.WaitCursor;
			GridHelper.GridlockAllAbsences(_grid, _parent.LockManager);
			_parent.Refresh();
			_parent.RefreshSelection();
			_parent.Cursor = Cursors.Default;
		}



		internal void ToolStripMenuItemLockAbsencesClick(object sender, EventArgs e)
		{
			lockAbsence(sender);
		}

		internal void ToolStripMenuItemAbsenceLockRmMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			lockAbsence(sender);
		}

		private void lockAbsence(object sender)
		{
			_parent.Cursor = Cursors.WaitCursor;
			var absence = (Absence)((ToolStripMenuItem)sender).Tag;
			GridHelper.GridlockAbsences(_grid, _parent.LockManager, absence);
			_parent.Refresh();
			_parent.RefreshSelection();
			_parent.Cursor = Cursors.Default;
		}

		internal void ToolStripMenuItemLockTag(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			_parent.Cursor = Cursors.WaitCursor;
			var scheduleTag = (IScheduleTag)((ToolStripMenuItem)sender).Tag;
			IGridSchedulesExtractor gridSchedulesExtractor = new GridSchedulesExtractor(_grid);
			IScheduleDayTagExtractor scheduleDayTagExtractor = new ScheduleDayTagExtractor(gridSchedulesExtractor.Extract());
			var gridlockTagCommand = new GridlockTagCommand(_parent.LockManager, scheduleDayTagExtractor, scheduleTag);
			gridlockTagCommand.Execute();
			_parent.Refresh();
			_parent.RefreshSelection();
			_parent.Cursor = Cursors.Default;
		}

		internal void ToolStripMenuItemAllFulFilledAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledAvailability(e.Button);
		}

		internal void ToolStripMenuItemAllAvailableAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllAvailableAvailability(e.Button);
		}

		internal void ToolStripMenuItemAllUnavailableAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllUnavailableAvailability(e.Button);
		}

		internal void ToolStripMenuItemAllFulFilledStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledStudentAvailability(e.Button);
		}

		internal void ToolStripMenuItemAllAvailableStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllAvailableStudentAvailability(e.Button);
		}

		internal void ToolStripMenuItemAllUnavailableStudentAvailabilityMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllUnavailableStudentAvailability(e.Button);
		}

		internal void ToolStripMenuItemAllRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllRotations(e.Button);
		}

		internal void ToolStripMenuItemAllDaysOffRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllDaysOffRotations(e.Button);
		}

		internal void ToolStripMenuItemAllShiftsRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllShiftsRotations(e.Button);
		}

		internal void ToolStripMenuItemAllFulFilledRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledRotations(e.Button);
		}

		internal void ToolStripMenuItemAllFulFilledDaysOffRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledDaysOffRotations(e.Button);
		}

		internal void ToolStripMenuItemAllFulFilledShiftsRotationsMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledShiftsRotations(e.Button);
		}

		internal void ToolStripMenuItemAllPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllPreferences(e.Button);
		}

		internal void ToolStripMenuItemAllDaysOffMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllDaysOff(e.Button);
		}

		internal void ToolStripMenuItemAllShiftsPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllShiftsPreferences(e.Button);
		}

		internal void ToolStripMenuItemAllMustHaveMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllMustHave(e.Button);
		}

		internal void ToolStripMenuItemAllFulfilledMustHaveMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledMustHave(e.Button);
		}

		internal void ToolStripMenuItemAllFulFilledPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledAbsencePreferences(e.Button);
		}

		internal void ToolStripMenuItemAllAbsencePreferenceMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllAbsencePreference(e.Button);
		}

		internal void ToolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledDaysOffPreferences(e.Button);
		}

		internal void ToolStripMenuItemAllFulFilledShiftsPreferencesMouseUp(object sender, MouseEventArgs e)
		{
			var executer = new LockExecuter(_grid, _restrictionExtractor, _parent.LockManager, _parent);
			executer.AllFulfilledShiftsPreferences(e.Button);
		}
	}
}