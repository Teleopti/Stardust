using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public  class IntervalDataMedianCalculator : IIntervalDataCalculator
    {
        public double Calculate(List<double> intervalValue)
        {
            double[] sortedPNumbers = intervalValue.ToArray() ;
            intervalValue.CopyTo(sortedPNumbers, 0);
            Array.Sort(sortedPNumbers);
            //get the median
            int size = sortedPNumbers.Length;
            int mid = size / 2;
            double median = (size % 2 != 0) ? (double)sortedPNumbers[mid] : ((double)sortedPNumbers[mid] + (double)sortedPNumbers[mid - 1]) / 2;
            return median;
        }
    }

    public interface IIntervalDataCalculator
    {
        double Calculate(List<double> intervalValue);
    }
}