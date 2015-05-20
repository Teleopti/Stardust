using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class CurrentScheduleSummaryCalculator
	{
		public Tuple<TimeSpan, int> SetCurrent(IScheduleRange scheduleRange)
		{
			var contractTime = TimeSpan.Zero;
			var numberOfDaysOff = 0;
			var person = scheduleRange.Person;
			var period = scheduleRange.Owner.Period.VisiblePeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());			
			foreach (var dateOnly in period.DayCollection())
			{
				var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
				if (!person.IsAgent(dateOnly))
					continue;
		
				var significantPart = scheduleDay.SignificantPartForDisplay();
				if (significantPart == SchedulePartView.ContractDayOff || significantPart == SchedulePartView.DayOff)
				{
					numberOfDaysOff++;
					continue;
				}

				contractTime = contractTime.Add(scheduleDay.ProjectionService().CreateProjection().ContractTime());
			}

			return new Tuple<TimeSpan, int>(contractTime,numberOfDaysOff);
		}
	}
}