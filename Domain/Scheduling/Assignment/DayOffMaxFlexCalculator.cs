using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IDayOffMaxFlexCalculator
	{
		DateTimePeriod? MaxFlex(IScheduleDay dayOffDay, IScheduleDay scheduleDay);
	}

	public class DayOffMaxFlexCalculator : IDayOffMaxFlexCalculator
	{
		private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;

		public DayOffMaxFlexCalculator(IWorkTimeStartEndExtractor workTimeStartEndExtractor)
		{
			_workTimeStartEndExtractor = workTimeStartEndExtractor;
		}

		public DateTimePeriod? MaxFlex(IScheduleDay dayOffDay, IScheduleDay scheduleDay)
		{
			if (dayOffDay == null || scheduleDay == null) return null;
			if (!dayOffDay.HasDayOff()) return null;
			
			var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var dayOffDateOnly = dayOffDay.DateOnlyAsPeriod.DateOnly;
			if (dayOffDateOnly == dateOnly) return null;

			var personAssignmentDayOffDay = dayOffDay.PersonAssignment();
			var dayOff = personAssignmentDayOffDay.DayOff();

			var startTime = dayOff.Boundary.StartDateTime;
			var endTime = startTime.Add(dayOff.TargetLength);
			var dayBefore = dayOffDateOnly > dateOnly;

			if (!dayBefore)
			{
				endTime = dayOff.Boundary.EndDateTime;
				startTime = endTime.Add(-dayOff.TargetLength);
			}

			var fullFlex = new DateTimePeriod(startTime, endTime);

			if (!scheduleDay.HasProjection())
				return fullFlex;

			var projection = scheduleDay.ProjectionService().CreateProjection();
			return dayBefore ? adjustedFlexPeriodBefore(dayOff, projection, fullFlex) : adjustedFlexPeriodAfter(dayOff, projection, fullFlex);
		}

		private DateTimePeriod adjustedFlexPeriodBefore(DayOff dayOff, IEnumerable<IVisualLayer> projection, DateTimePeriod fullFlex)
		{
			var endTime = _workTimeStartEndExtractor.WorkTimeEnd(projection);
			if (endTime == null)
				return fullFlex;

			return endTime.Value < fullFlex.StartDateTime ? fullFlex : new DateTimePeriod(endTime.Value, endTime.Value.Add(dayOff.TargetLength));
		}

		private DateTimePeriod adjustedFlexPeriodAfter(DayOff dayOff, IEnumerable<IVisualLayer> projection, DateTimePeriod fullFlex)
		{
			var startTime = _workTimeStartEndExtractor.WorkTimeStart(projection);
			if (startTime == null) 
				return fullFlex;

			return startTime.Value > fullFlex.EndDateTime ? fullFlex : new DateTimePeriod(startTime.Value.Add(-dayOff.TargetLength), startTime.Value);
		}
	}
}
