using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TargetScheduleSummaryCalculator
	{
		public Tuple<TimeSpan?, int?> GetTargets(IScheduleRange range, DateOnlyPeriod visiblePeriod)
		{
			var targetTime = TimeSpan.Zero;
			var targetDaysOff = 0;
			var person = range.Person;
			var schedulePeriods = extractVirtualPeriods(person, visiblePeriod);
			foreach (var virtualSchedulePeriod in schedulePeriods)
			{
				if(!virtualSchedulePeriod.IsValid) continue;
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