using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{

    /// <summary>
    /// Holds info of when, who and on what scenario
    /// a persons schedule belongs to
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-11
    /// </remarks>
    public class ScheduleParameters : IScheduleParameters
    {
        private readonly IPerson _person;
        private readonly IScenario _scenario; 
        private readonly DateTimePeriod _period;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleParameters"/> class.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <param name="person">The person.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-11
        /// </remarks>
        public ScheduleParameters(IScenario scenario, IPerson person, DateTimePeriod period)
        {
            InParameter.NotNull("scenario", scenario);
            InParameter.NotNull("person", person);
            _person = person;
            _scenario = scenario;
            _period = period;
        }

        /// <summary>
        /// Gets the scheduled person.
        /// </summary>
        /// <value>The scheduled person.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-06
        /// </remarks>
        public IPerson Person
        {
            get { return _person; }
        }

        /// <summary>
        /// Gets the scenario.
        /// </summary>
        /// <value>The scenario.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-06
        /// </remarks>
        public IScenario Scenario
        {
            get { return _scenario; }
        }

        /// <summary>
        /// Gets the Period.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-06
        /// </remarks>
        public DateTimePeriod Period
        {
            get { return _period; }
        }
    }
}
