using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalLengthServiceLevelSpecification : Specification<double>
    {
        private readonly int _resolution;

        public IntervalLengthServiceLevelSpecification(int resolution)
        {
            _resolution = resolution;
        }

        public override bool IsSatisfiedBy(double obj)
        {
            return obj / (_resolution * TimeDefinition.SecondsPerMinute) <= TimeDefinition.HoursPerDay;
        }
    }
}