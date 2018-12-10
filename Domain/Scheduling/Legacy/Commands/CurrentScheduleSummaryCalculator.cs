using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class CurrentScheduleSummaryCalculator
	{
		public CurrentScheduleSummary GetCurrent(IScheduleRange scheduleRange, DateOnlyPeriod period)
		{
			var person = scheduleRange.Person;
			return calculate(scheduleRange, period, person);
		}

		private CurrentScheduleSummary calculate(IScheduleRange scheduleRange, DateOnlyPeriod period, IPerson person)
		{
			var result = new CurrentScheduleSummary();
			foreach (var scheduleDay in scheduleRange.ScheduledDayCollection(period))
			{
				if (!person.IsAgent(scheduleDay.DateOnlyAsPeriod.DateOnly))
					continue;
				
				var significantPart = scheduleDay.SignificantPartForDisplay();
				if (significantPart == SchedulePartView.ContractDayOff || significantPart == SchedulePartView.DayOff)
				{
					result.NumberOfDaysOff++;
					continue;
				}

				var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
				if (!visualLayerCollection.HasLayers)
					result.DaysWithoutSchedule++;
				result.ContractTime = result.ContractTime.Add(visualLayerCollection.ContractTime());
			}
			return result;	
		}
	}

	public class CurrentScheduleSummary
	{
		public TimeSpan ContractTime { get; set; }
		public int NumberOfDaysOff { get; set; }
		public int DaysWithoutSchedule { get; set; }
	}
}