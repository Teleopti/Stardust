using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SwapService : ISwapService
	{
		private IList<IScheduleDay> _selectedSchedules;

		public void Init(IList<IScheduleDay> selectedSchedules)
		{
			_selectedSchedules = selectedSchedules;
		}

		public bool CanSwapAssignments()
		{
			if(!checkBasicRules())
				return false;
			return true;
		}

		public IList<IScheduleDay> SwapAssignments(IScheduleDictionary schedules, bool ignoreAssignmentPermission, ITimeZoneGuard timeZoneGuard)
		{
			if(!CanSwapAssignments())
				throw new ConstraintException("Can not swap assignments");

			var retList = new List<IScheduleDay>();
			var toPersonSchedule = _selectedSchedules[1];
			var fromPersonSchedule = _selectedSchedules[0];

			var toPersonSchedulePart = schedules[toPersonSchedule.Person].ReFetch(toPersonSchedule);
			var fromPersonSchedulePart = schedules[fromPersonSchedule.Person].ReFetch(fromPersonSchedule);
			var toPersonAssignment = toPersonSchedulePart.PersonAssignment();
			var fromPersonAssignment = fromPersonSchedulePart.PersonAssignment();

			removeAbsences(toPersonSchedulePart);
			removeAbsences(fromPersonSchedulePart);

			if ((toPersonAssignment==null || fromPersonAssignment==null) && !toPersonSchedulePart.HasDayOff() && !fromPersonSchedulePart.HasDayOff())
			{
				if (toPersonAssignment == null)
				{
					toPersonSchedule.Merge(fromPersonSchedulePart, false,true, timeZoneGuard);
					toPersonSchedule.DeletePersonalStuff();
					fromPersonSchedule.DeleteMainShift();
				}
				else
				{
					fromPersonSchedule.Merge(toPersonSchedulePart, false,true, timeZoneGuard);
					fromPersonSchedule.DeletePersonalStuff();
					toPersonSchedule.DeleteMainShift();
				}
			}
			else
			{
				if (!fromPersonSchedulePart.IsScheduled() && toPersonSchedulePart.IsScheduled())
				{
					fromPersonSchedule.Merge(toPersonSchedulePart, false, true, timeZoneGuard);
					toPersonSchedule.DeleteMainShift();
					toPersonSchedule.DeleteDayOff();
				}
				else if (!toPersonSchedulePart.IsScheduled() && fromPersonSchedulePart.IsScheduled())
				{
					toPersonSchedule.Merge(fromPersonSchedulePart, false, true, timeZoneGuard);
					fromPersonSchedule.DeleteMainShift();
					fromPersonSchedule.DeleteDayOff();
				}
				else
				{
					fromPersonSchedule.Merge(toPersonSchedulePart, false, true, timeZoneGuard, ignoreAssignmentPermission);
					toPersonSchedule.Merge(fromPersonSchedulePart, false, true, timeZoneGuard, ignoreAssignmentPermission);
				}
			}
			retList.AddRange(_selectedSchedules);
			return retList;
		}

		private static void removeAbsences(IScheduleDay schedulePart)
		{
			if (schedulePart.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence)
			{
				var personAbsences = schedulePart.PersonAbsenceCollection();
				foreach (var personAbsence in personAbsences)
				{
					schedulePart.Remove(personAbsence);
				}
			}
		}

		private bool checkBasicRules()
		{
			if (_selectedSchedules.Count != 2)
				return false;

			if (_selectedSchedules[0].Person == _selectedSchedules[1].Person)
				return false;

			return true;
		}
	}
}