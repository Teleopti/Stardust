using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    

    public class IntervalDataEntendedMedianCalculator : IIntervalDataCalculator
    {
        public double Calculate(IList<double> intervalValue)
        {
            if (intervalValue != null && intervalValue.Count == 0) return 0f;
            var sortedPNumbers = intervalValue.ToArray();
            if (intervalValue != null) intervalValue.CopyTo(sortedPNumbers, 0);
            Array.Sort(sortedPNumbers);
            var size = sortedPNumbers.Length;
            var mid = size / 2;
            //get the minimum number with maximum occurances
            var sortedNumber = new Dictionary<double, int>();
            for (int i = mid; i < size-1; i++)
            {
                if (sortedNumber.ContainsKey(sortedPNumbers[i]))
                    sortedNumber[sortedPNumbers[i]] = sortedNumber[sortedPNumbers[i]] +1;
                else
                    sortedNumber.Add(sortedPNumbers[i],0);
            }
            var  myList = sortedNumber.ToList();
            myList.Sort(
                (firstPair, nextPair) => nextPair.Value.CompareTo(firstPair .Value)
                );

            return myList.First().Key ;
        }
    }
}