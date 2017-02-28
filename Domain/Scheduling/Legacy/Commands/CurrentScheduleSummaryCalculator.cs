using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class CurrentScheduleSummaryCalculator
	{
		public Tuple<TimeSpan, int> GetCurrent(IScheduleRange scheduleRange, DateOnlyPeriod period)
		{
			var person = scheduleRange.Person;
			return calculate(scheduleRange, period, person);
		}

		private Tuple<TimeSpan, int> calculate(IScheduleRange scheduleRange, DateOnlyPeriod period, IPerson person)
		{
			var contractTime = TimeSpan.Zero;
			var numberOfDaysOff = 0;
			
			foreach (var scheduleDay in scheduleRange.ScheduledDayCollection(period))
			{
				if (!person.IsAgent(scheduleDay.DateOnlyAsPeriod.DateOnly))
					continue;
				
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