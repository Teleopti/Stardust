using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public interface IShiftFairnessCalculator
    {
        IList<ShiftFairness> GetShiftFairness();
    }

    public class ShiftFairnessCalculator : IShiftFairnessCalculator
    {
        private readonly IEnumerable<ShiftCategoryPerAgent> _shiftCategoryPerAgentList;

        public ShiftFairnessCalculator(IEnumerable<ShiftCategoryPerAgent> shiftCategoryPerAgentList)
        {
            _shiftCategoryPerAgentList = shiftCategoryPerAgentList;
            
        }

        public IList<ShiftFairness> GetShiftFairness()
        {
            var returnShiftFairness = new List<ShiftFairness>();
            var shiftCategoryPerAgentCountValues = new Dictionary<string, List<double>>();
            foreach (var shiftCategoryPerAgent in _shiftCategoryPerAgentList)
            {
                if (shiftCategoryPerAgentCountValues.ContainsKey(shiftCategoryPerAgent.ShiftCategoryName))
                {
                    shiftCategoryPerAgentCountValues[shiftCategoryPerAgent.ShiftCategoryName].Add(shiftCategoryPerAgent.Count);
                }
                else
                {
                    shiftCategoryPerAgentCountValues.Add(shiftCategoryPerAgent.ShiftCategoryName, new List<double>());
                    shiftCategoryPerAgentCountValues[shiftCategoryPerAgent.ShiftCategoryName].Add(shiftCategoryPerAgent.Count);
                }
            }

            foreach (var scPerAgent in shiftCategoryPerAgentCountValues)
            {
                var min = scPerAgent.Value.Min();
                var max = scPerAgent.Value.Max();
                var avg = scPerAgent.Value.Average();
                var std = getStandardDeviation(scPerAgent.Value);
                returnShiftFairness.Add(new ShiftFairness(scPerAgent.Key,(int) min ,(int) max,avg,std ));
            }
            return returnShiftFairness;
        }

        private double getStandardDeviation(List<double> doubleList)
        {
            double average = doubleList.Average();
            double sumOfDerivation = 0;
            foreach (double value in doubleList)
            {
                sumOfDerivation += (value) * (value);
            }
            double sumOfDerivationAverage = sumOfDerivation / doubleList.Count;
            return Math.Sqrt(sumOfDerivationAverage - (average * average));
        }  
        
    }
}