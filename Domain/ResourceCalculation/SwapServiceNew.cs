using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class SwapServiceNew : ISwapServiceNew
	{
		private IList<IScheduleDay> _selectedSchedules;

		public void Init(IList<IScheduleDay> selectedSchedules)
		{
			_selectedSchedules = selectedSchedules;
		}

		public bool CanSwapAssignments()
		{
			if (!CheckBasicRules())
				return false;
			return true;
		}

			public IList<IScheduleDay> Swap(IScheduleDictionary schedules)
		{
			if(schedules == null)
				throw new ArgumentNullException("schedules");

			if (!CanSwapAssignments())
				throw new ConstraintException("Can not swap assignments");

			var retList = new List<IScheduleDay>();

			var schedulePart0 = schedules[_selectedSchedules[0].Person].ReFetch(_selectedSchedules[0]);
			var schedulePart1 = schedules[_selectedSchedules[1].Person].ReFetch(_selectedSchedules[1]);

			var assignment0 = schedulePart0.PersonAssignment(true);
			var assignment1 = schedulePart1.PersonAssignment(true);
			var workingAssignment = assignment0.NoneEntityClone();

			movePersonAssignment(assignment0, workingAssignment);
			movePersonAssignment(assignment1, assignment0);
			movePersonAssignment(workingAssignment, assignment1);

			retList.AddRange(new List<IScheduleDay> {schedulePart0, schedulePart1});
			return retList;
		}

		private static void movePersonAssignment(IPersonAssignment sourceAssignment, IPersonAssignment targetAssignment)
		{

			//var periodOffsetCalculator = new PeriodOffsetCalculator();
			//var periodOffset = periodOffsetCalculator.CalculatePeriodOffset(sourceAssignment.Period, targetAssignment.Period);

			targetAssignment.ClearMainActivities();
			targetAssignment.ClearOvertimeActivities();

			foreach (var layer in sourceAssignment.MainActivities())
			{
				targetAssignment.AddActivity(layer.Payload, layer.Period);
			}

			var timeZoneInfo = sourceAssignment.Person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(sourceAssignment.Period.StartDateTime, timeZoneInfo));
			var personPeriod = sourceAssignment.Person.Period(dateOnlyPerson);

			foreach (var layer in sourceAssignment.OvertimeActivities())
			{
				if (personPeriod.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Contains(layer.DefinitionSet))
				{
					targetAssignment.AddOvertimeActivity(layer.Payload, layer.Period, layer.DefinitionSet);
				}
			}

			sourceAssignment.SetThisAssignmentsDayOffOn(targetAssignment);

			targetAssignment.SetShiftCategory(sourceAssignment.ShiftCategory);

			sourceAssignment.ClearMainActivities();
			sourceAssignment.ClearOvertimeActivities();
			sourceAssignment.SetDayOff(null);
		}

		public IList<IScheduleDay> Swap(IList<IScheduleDay> selectedSchedules, IScheduleDictionary schedules)
		{
			Init(selectedSchedules);
			return Swap(schedules);
		}

		private bool CheckBasicRules()
		{
			if (_selectedSchedules.Count != 2)
				return false;

			if (_selectedSchedules[0].Person == _selectedSchedules[1].Person)
				return false;

			return true;
		}
	}
}
