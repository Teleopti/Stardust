using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class SwapServiceNew : ISwapServiceNew
	{
		public IList<IScheduleDay> Swap(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2, IScheduleDictionary schedules)
		{
			InParameter.NotNull("schedules", schedules);

			if (!canSwapAssignments(scheduleDay1, scheduleDay2))
				throw new ConstraintException("Can not swap assignments");

			var retList = new List<IScheduleDay>();

			var schedulePart1 = schedules[scheduleDay1.Person].ReFetch(scheduleDay1);
			var schedulePart2 = schedules[scheduleDay2.Person].ReFetch(scheduleDay2);

			swapMainShiftAndOvertime(schedulePart1, schedulePart2);
			swapDayOff(schedulePart1, schedulePart2);

			retList.AddRange(new List<IScheduleDay> { schedulePart1, schedulePart2 });
			return retList;
		}

		private void swapMainShiftAndOvertime(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2)
		{
			var tempSlot = (IScheduleDay)scheduleDay1.Clone();
			tempSlot.Clear<IScheduleData>();

			copyAndClearMainShiftAndOvertime(scheduleDay1, tempSlot);
			copyAndClearMainShiftAndOvertime(scheduleDay2, scheduleDay1);
			copyAndClearMainShiftAndOvertime(tempSlot, scheduleDay2);

		}

		private void swapDayOff(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2)
		{
			var tempSlot = (IScheduleDay)scheduleDay1.Clone();
			tempSlot.Clear<IScheduleData>();

			copyAndClearDayOff(scheduleDay1, tempSlot);
			copyAndClearDayOff(scheduleDay2, scheduleDay1);
			copyAndClearDayOff(tempSlot, scheduleDay2);
		}

		private static void copyAndClearMainShiftAndOvertime(IScheduleDay slot1, IScheduleDay slot2)
		{
			foreach (var personAssignment in slot1.PersonAssignmentCollection())
			{
				var mainShift = personAssignment.MainShift;
				if (mainShift != null)
				{
					slot2.AddMainShift((IMainShift) mainShift.NoneEntityClone());
				}

				var overtimeShiftToRemove = new List<IOvertimeShift>();
				foreach (var overtimeShift in personAssignment.OvertimeShiftCollection)
				{
					overtimeShiftToRemove.Add(overtimeShift);
					foreach (var layer in overtimeShift.LayerCollectionWithDefinitionSet())
					{
						slot2.CreateAndAddOvertime(layer);
					}
				}
				foreach (var overtimeShift in overtimeShiftToRemove)
				{
					personAssignment.RemoveOvertimeShift(overtimeShift);
				}
			}
			slot1.DeleteMainShift(slot1);
		}

		private static void copyAndClearDayOff(IScheduleDay scheduleDay1, IScheduleDay tempSlot)
		{
			foreach (var personDayOff in scheduleDay1.PersonDayOffCollection())
			{
				var oldDayOff = personDayOff.DayOff;
				var dayOff = new DayOff(oldDayOff.Anchor, oldDayOff.TargetLength, oldDayOff.Flexibility, oldDayOff.Description,
				                        oldDayOff.DisplayColor, oldDayOff.PayrollCode);
				var personDayOffToAdd = new PersonDayOff(tempSlot.Person, tempSlot.Scenario, dayOff,
				                                         tempSlot.DateOnlyAsPeriod.DateOnly, scheduleDay1.TimeZone);
				tempSlot.Add(personDayOffToAdd);
			}
			scheduleDay1.DeleteDayOff();
		}

		private bool canSwapAssignments(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2)
		{
			if (!checkBasicRules(scheduleDay1, scheduleDay2))
				return false;
			return true;
		}

		private bool checkBasicRules(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2)
		{
			if (scheduleDay1 == null || scheduleDay2 == null)
				return false;

			if (scheduleDay1.Person == scheduleDay2.Person)
				return false;

			return true;
		}
	}
}
