using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class ScheduleTargetTimeCalculator : IScheduleTargetTimeCalculator
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IPerson _person;
		private readonly DateOnlyPeriod _dateOnlyPeriod;

		public ScheduleTargetTimeCalculator(
            ISchedulerStateHolder schedulerStateHolder, 
            IPerson person, 
            DateOnlyPeriod dateOnlyPeriod)
		{
			if (schedulerStateHolder == null)
				throw new ArgumentNullException("schedulerStateHolder");

			if (person == null)
				throw new ArgumentNullException("person");

			_schedulerStateHolder = schedulerStateHolder;
			_person = person;
			_dateOnlyPeriod = dateOnlyPeriod;
		}

		public TimeSpan CalculateTargetTime()
		{
			var virtualSchedulePeriod = _person.VirtualSchedulePeriod(_dateOnlyPeriod.StartDate);
            var fullWeekOuterWeekPeriodCreator = 
                new FullWeekOuterWeekPeriodCreator(_dateOnlyPeriod, _person);
			var matrix = 
                new ScheduleMatrixPro(_schedulerStateHolder.SchedulingResultState.Schedules, fullWeekOuterWeekPeriodCreator, virtualSchedulePeriod);
			var schedulePeriodTargetCalculatorFactory = 
                new NewSchedulePeriodTargetCalculatorFactory(matrix);
			var schedulePeriodTargetCalculator = 
                schedulePeriodTargetCalculatorFactory.CreatePeriodTargetCalculator();
			return schedulePeriodTargetCalculator == null 
                ? TimeSpan.Zero 
                : schedulePeriodTargetCalculator.PeriodTarget(false);
		}
	}
}
