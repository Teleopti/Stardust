using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class ScheduleParameters : IScheduleParameters
    {
        public ScheduleParameters(IScenario scenario, IPerson person, DateTimePeriod period)
        {
            InParameter.NotNull(nameof(scenario), scenario);
            InParameter.NotNull(nameof(person), person);

            Person = person;
            Scenario = scenario;
            Period = period;
        }
		
        public IPerson Person { get; }
		
        public IScenario Scenario { get; }
		
        public DateTimePeriod Period { get; }
    }
}
