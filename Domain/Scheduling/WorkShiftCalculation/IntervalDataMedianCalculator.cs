using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IIntervalDataCalculator
    {
        double Calculate(IList<double> intervalValue);
    }

    public  class IntervalDataMedianCalculator : IIntervalDataCalculator
    {
        public double Calculate(IList<double> intervalValue)
        {
            if (intervalValue.Count == 0) return 0f;
            var sortedPNumbers = intervalValue.ToArray() ;
            intervalValue.CopyTo(sortedPNumbers, 0);
            Array.Sort(sortedPNumbers);
            var size = sortedPNumbers.Length;
            var mid = size / 2;
            var median = (size % 2 != 0) ? sortedPNumbers[mid] : (sortedPNumbers[mid] + sortedPNumbers[mid - 1]) / 2;
            return median;
        }
    }
}