﻿using System;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
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
            var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
			var virtualSchedulePeriod = 
                new VirtualSchedulePeriod(_person, _dateOnlyPeriod.StartDate, splitChecker);
            var fullWeekOuterWeekPeriodCreator = 
                new FullWeekOuterWeekPeriodCreator(_dateOnlyPeriod, _person);
			var matrix = 
                new ScheduleMatrixPro(_schedulerStateHolder.SchedulingResultState, fullWeekOuterWeekPeriodCreator, virtualSchedulePeriod);
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
