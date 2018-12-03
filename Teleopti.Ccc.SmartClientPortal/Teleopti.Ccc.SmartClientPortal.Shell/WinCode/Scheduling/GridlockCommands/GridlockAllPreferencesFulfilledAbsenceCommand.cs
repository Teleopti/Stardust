using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands
{
	public class GridlockAllPreferencesFulfilledAbsenceCommand
	{
		private readonly IGridlockManager _lockManager;
		private readonly IScheduleDayPreferenceRestrictionExtractor _scheduleDayPreferenceRestrictionExtractor;
		private readonly ICheckerRestriction _restrictionChecker;
		private readonly IGridSchedulesExtractor _schedulesExtractor;

		public GridlockAllPreferencesFulfilledAbsenceCommand(IGridSchedulesExtractor schedulesExtractor, ICheckerRestriction restrictionChecker, IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor, IGridlockManager lockManager)
		{
			_lockManager = lockManager;
			_scheduleDayPreferenceRestrictionExtractor = scheduleDayPreferenceRestrictionExtractor;
			_restrictionChecker = restrictionChecker;
			_schedulesExtractor = schedulesExtractor;
		}

		public void Execute()
		{
			var scheduleDays = _schedulesExtractor.Extract();
			var restrictedDays = new List<IScheduleDay>();

			foreach (var scheduleDay in scheduleDays)
			{
				var restrictedDay = _scheduleDayPreferenceRestrictionExtractor.RestrictionFulfilledAbsence(_restrictionChecker, scheduleDay);

				if (restrictedDay != null)
					restrictedDays.Add(scheduleDay);
			}

			_lockManager.AddLock(restrictedDays, LockType.Normal);
		}
	}
}