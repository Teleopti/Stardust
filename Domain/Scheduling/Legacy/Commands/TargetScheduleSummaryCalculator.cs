using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TargetScheduleSummaryCalculator
	{
		public Tuple<TimeSpan?, int?> GetTargets(IScheduleRange range)
		{
			var targetTime = TimeSpan.Zero;
			var targetDaysOff = 0;
			var person = range.Person;
			var period = range.Owner.Period.VisiblePeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			var schedulePeriods = extractVirtualPeriods(person, period);
			foreach (var virtualSchedulePeriod in schedulePeriods)
			{
				IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
						new FullWeekOuterWeekPeriodCreator(virtualSchedulePeriod.DateOnlyPeriod, virtualSchedulePeriod.Person);
				IScheduleMatrixPro matrix = new ScheduleMatrixPro(range, fullWeekOuterWeekPeriodCreator, virtualSchedulePeriod);
				ISchedulePeriodTargetCalculatorFactory schedulePeriodTargetCalculatorFactory =
					new NewSchedulePeriodTargetCalculatorFactory(matrix);
				ISchedulePeriodTargetCalculator calculator = schedulePeriodTargetCalculatorFactory.CreatePeriodTargetCalculator();
				if (calculator == null)
					return new Tuple<TimeSpan?, int?>(null, null);

				targetTime = targetTime.Add(calculator.PeriodTarget(true));
				targetDaysOff += virtualSchedulePeriod.DaysOff();
			}
			return new Tuple<TimeSpan?, int?>(targetTime, targetDaysOff);
		}

		private static IEnumerable<IVirtualSchedulePeriod> extractVirtualPeriods(IPerson person, DateOnlyPeriod period)
		{
			if (person == null)
				throw new ArgumentNullException("person");

			var virtualPeriods = new HashSet<IVirtualSchedulePeriod>();
			foreach (var dateOnly in period.DayCollection())
			{
				virtualPeriods.Add(person.VirtualSchedulePeriod(dateOnly));
			}
			return virtualPeriods;
		}
	}
}