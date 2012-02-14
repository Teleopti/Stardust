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
            IPopulationStatisticsCalculator stddevCalculator = new PopulationStatisticsCalculator();
            double totalValue = scheduleDictionary.FairnessPoints().FairnessPointsPerShift;
            foreach (IPerson person in scheduleDictionary.Keys)
            {
                double value = calcPersonFainess(person, totalValue);
                stddevCalculator.AddItem(value);
            }
            
            stddevCalculator.Analyze();
            return stddevCalculator.StandardDeviation;
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