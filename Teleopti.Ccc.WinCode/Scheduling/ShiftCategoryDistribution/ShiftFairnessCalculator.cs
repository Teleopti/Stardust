using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{

    public static class ShiftFairnessCalculator 
    {

        public static IList<ShiftFairness> GetShiftFairness(IEnumerable<ShiftCategoryPerAgent> shiftCategoryPerAgentList)
        {
            var returnShiftFairness = new List<ShiftFairness>();
            var shiftCategoryPerAgentCountValues = new Dictionary<IShiftCategory, List<double>>();
            foreach (var shiftCategoryPerAgent in shiftCategoryPerAgentList)
            {
                if (shiftCategoryPerAgentCountValues.ContainsKey(shiftCategoryPerAgent.ShiftCategory))
                {
                    shiftCategoryPerAgentCountValues[shiftCategoryPerAgent.ShiftCategory].Add(shiftCategoryPerAgent.Count);
                }
                else
                {
                    shiftCategoryPerAgentCountValues.Add(shiftCategoryPerAgent.ShiftCategory, new List<double>());
                    shiftCategoryPerAgentCountValues[shiftCategoryPerAgent.ShiftCategory].Add(shiftCategoryPerAgent.Count);
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

        private static double getStandardDeviation(List<double> doubleList)
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