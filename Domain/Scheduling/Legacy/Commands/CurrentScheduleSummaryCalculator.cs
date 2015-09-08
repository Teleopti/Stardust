using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class CurrentScheduleSummaryCalculator
	{
		public Tuple<TimeSpan, int> GetCurrent(IScheduleRange scheduleRange)
		{
			var person = scheduleRange.Person;
			var period = scheduleRange.Owner.Period.VisiblePeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			return calculate(scheduleRange, period, person);
		}

		public Tuple<TimeSpan, int> GetCurrent(IScheduleRange scheduleRange, DateOnlyPeriod period)
		{
			var person = scheduleRange.Person;
			return calculate(scheduleRange, period, person);
		}

		private Tuple<TimeSpan, int> calculate(IScheduleRange scheduleRange, DateOnlyPeriod period, IPerson person)
		{
			var contractTime = TimeSpan.Zero;
			var numberOfDaysOff = 0;
			
			foreach (var dateOnly in period.DayCollection())
			{
				if (!person.IsAgent(dateOnly))
					continue;

				var scheduleDay = scheduleRange.ScheduledDay(dateOnly);

				var significantPart = scheduleDay.SignificantPartForDisplay();
				if (significantPart == SchedulePartView.ContractDayOff || significantPart == SchedulePartView.DayOff)
				{
					numberOfDaysOff++;
					continue;
				}

				contractTime = contractTime.Add(scheduleDay.ProjectionService().CreateProjection().ContractTime());
			}

			return new Tuple<TimeSpan, int>(contractTime, numberOfDaysOff);	
		}
	}
}