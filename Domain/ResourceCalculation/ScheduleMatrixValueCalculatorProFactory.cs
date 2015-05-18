﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ScheduleMatrixValueCalculatorProFactory : IScheduleMatrixValueCalculatorProFactory
    {
        public IScheduleMatrixValueCalculatorPro CreateScheduleMatrixValueCalculatorPro(IEnumerable<DateOnly> scheduleDays, IOptimizationPreferences optimizerPreferences, ISchedulingResultStateHolder stateHolder)
        {
            return new ScheduleMatrixValueCalculatorPro
                (scheduleDays,
                 optimizerPreferences,
                 stateHolder);
        }
    }
}
