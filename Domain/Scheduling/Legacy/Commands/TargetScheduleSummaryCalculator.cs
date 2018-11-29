using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TargetScheduleSummaryCalculator
	{
		public TargetScheduleSummary GetTargets(IScheduleRange range, DateOnlyPeriod visiblePeriod)
		{
			var result = new TargetScheduleSummary();
			var person = range.Person;
			var schedulePeriods = extractVirtualPeriods(person, visiblePeriod);
			foreach (var virtualSchedulePeriod in schedulePeriods)
			{
				if(!virtualSchedulePeriod.IsValid) continue;
				IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
						new FullWeekOuterWeekPeriodCreator(virtualSchedulePeriod.DateOnlyPeriod, virtualSchedulePeriod.Person);
				IScheduleMatrixPro matrix = new ScheduleMatrixPro(range, fullWeekOuterWeekPeriodCreator, virtualSchedulePeriod);
				var schedulePeriodTargetCalculatorFactory = new NewSchedulePeriodTargetCalculatorFactory(matrix);
				var calculator = schedulePeriodTargetCalculatorFactory.CreatePeriodTargetCalculator();
				if (calculator == null)
					return new TargetScheduleSummary();

				result.TargetTime = (result.TargetTime ?? TimeSpan.Zero).Add(calculator.PeriodTarget(true));
				result.TargetDaysOff = (result.TargetDaysOff ?? 0) + virtualSchedulePeriod.DaysOff();

				result.NegativeTargetTimeTolerance += virtualSchedulePeriod.Contract.NegativePeriodWorkTimeTolerance;
				result.PositiveTargetTimeTolerance += virtualSchedulePeriod.Contract.PositivePeriodWorkTimeTolerance;
				result.NegativeTargetDaysOffTolerance += virtualSchedulePeriod.Contract.NegativeDayOffTolerance;
				result.PositiveTargetDaysOffTolerance += virtualSchedulePeriod.Contract.PositiveDayOffTolerance;
			}
			return result;
		}

		private static IEnumerable<IVirtualSchedulePeriod> extractVirtualPeriods(IPerson person, DateOnlyPeriod period)
		{
			if (person == null)
				throw new ArgumentNullException(nameof(person));

			var virtualPeriods = new HashSet<IVirtualSchedulePeriod>();
			foreach (var dateOnly in period.DayCollection())
			{
				virtualPeriods.Add(person.VirtualSchedulePeriod(dateOnly));
			}
			return virtualPeriods;
		}
	}

	public class TargetScheduleSummary
	{
		public TimeSpan? TargetTime { get; set; }
		public int? TargetDaysOff { get; set; }
		public TimeSpan NegativeTargetTimeTolerance { get; set; }
		public TimeSpan PositiveTargetTimeTolerance { get; set; }
		public int PositiveTargetDaysOffTolerance { get; set; }
		public int NegativeTargetDaysOffTolerance { get; set; }
	}
}