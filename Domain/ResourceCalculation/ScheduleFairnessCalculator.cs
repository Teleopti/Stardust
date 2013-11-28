using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ScheduleFairnessCalculator : IScheduleFairnessCalculator
    {
        private readonly ISchedulingResultStateHolder _resultStateHolder;
        
        public ScheduleFairnessCalculator(ISchedulingResultStateHolder resultStateHolder)
        {
            _resultStateHolder = resultStateHolder;
        }

        public double PersonFairness(IPerson person)
        {
            double totalValue = _resultStateHolder.Schedules.FairnessPoints().FairnessPointsPerShift;
            return calcPersonFainess(person, totalValue);
        }

        public double ScheduleFairness()
        {
            var scheduleDictionary = _resultStateHolder.Schedules;
            double totalValue = scheduleDictionary.FairnessPoints().FairnessPointsPerShift;

	        var values = scheduleDictionary.Keys.Select(p => calcPersonFainess(p, totalValue)).ToArray();
            return Calculation.Variances.StandardDeviation(values);
        }

        private double calcPersonFainess(IPerson person, double totalValue)
        {
            double personValue = _resultStateHolder.Schedules[person].FairnessPoints().FairnessPointsPerShift;

            if (personValue == 0)
                return 1d;

            if (totalValue == 0)
                return 1d;

            return personValue / totalValue;
        }
    }
}