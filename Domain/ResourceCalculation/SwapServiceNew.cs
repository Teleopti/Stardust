using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SwapServiceNew : ISwapServiceNew
	{
		public bool CanSwapAssignments(IList<IScheduleDay> selectedSchedules)
		{
			if (!CheckBasicRules(selectedSchedules))
				return false;
			return true;
		}

		public IList<IScheduleDay> Swap(IScheduleDictionary schedules, IList<IScheduleDay> selectedSchedules, TrackedCommandInfo trackedCommandInfo = null)
		{
			if(schedules == null)
				throw new ArgumentNullException(nameof(schedules));

			if (!CanSwapAssignments(selectedSchedules))
				throw new ConstraintException("Can not swap assignments");

			var retList = new List<IScheduleDay>();

			var schedulePart0 = schedules[selectedSchedules[0].Person].ReFetch(selectedSchedules[0]);
			var schedulePart1 = schedules[selectedSchedules[1].Person].ReFetch(selectedSchedules[1]);

			var assignment0 = schedulePart0.PersonAssignment(true);
			var assignment1 = schedulePart1.PersonAssignment(true);
			var workingAssignment = assignment0.NoneEntityClone();

			movePersonAssignment(assignment0, workingAssignment, trackedCommandInfo);
			movePersonAssignment(assignment1, assignment0, trackedCommandInfo);
			movePersonAssignment(workingAssignment, assignment1, trackedCommandInfo);

			retList.AddRange(new List<IScheduleDay> {schedulePart0, schedulePart1});
			return retList;
		}

		private static void movePersonAssignment(IPersonAssignment sourceAssignment, IPersonAssignment targetAssignment, TrackedCommandInfo trackedCommandInfo)
		{
			targetAssignment.ClearMainActivities(false, trackedCommandInfo);
			targetAssignment.ClearOvertimeActivities(false, trackedCommandInfo);

			foreach (var layer in sourceAssignment.MainActivities())
			{
				targetAssignment.AddActivity(layer.Payload, layer.Period, trackedCommandInfo);
			}

			var dateOnlyPerson = sourceAssignment.Date;
			var personPeriod = sourceAssignment.Person.Period(dateOnlyPerson);

			foreach (var layer in sourceAssignment.OvertimeActivities())
			{
				if (personPeriod.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Contains(layer.DefinitionSet))
				{
					targetAssignment.AddOvertimeActivity(layer.Payload, layer.Period, layer.DefinitionSet);
				}
			}

			sourceAssignment.SetThisAssignmentsDayOffOn(targetAssignment,false, trackedCommandInfo);

			targetAssignment.SetShiftCategory(sourceAssignment.ShiftCategory);

			sourceAssignment.ClearMainActivities( false, trackedCommandInfo );
			sourceAssignment.ClearOvertimeActivities( false, trackedCommandInfo );
			sourceAssignment.SetDayOff(null, false, trackedCommandInfo);
		}

		public IList<IScheduleDay> Swap(IList<IScheduleDay> selectedSchedules, IScheduleDictionary schedules)
		{
			return Swap(schedules, selectedSchedules);
		}

		private bool CheckBasicRules(IList<IScheduleDay> selectedSchedules)
		{
			if (selectedSchedules.Count != 2)
				return false;

			if (selectedSchedules[0].Person == selectedSchedules[1].Person)
				return false;

			return true;
		}
	}
}
