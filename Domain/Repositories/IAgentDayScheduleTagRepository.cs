using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAgentDayScheduleTagRepository : IRepository<IAgentDayScheduleTag>, ILoadAggregateById<IAgentDayScheduleTag>, ILoadAggregateFromBroker<IAgentDayScheduleTag>
    {
        /// <summary>
        /// Finds all agent scheduleday tags for the specified period and scenario.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        IList<IAgentDayScheduleTag> Find(DateTimePeriod period, IScenario scenario);

        /// <summary>
        /// Finds all agent schedule day tags for the specified period, persons and scenario.
        /// </summary>
        /// <param name="dateOnlyPeriod">The date only period.</param>
        /// <param name="personCollection">The person collection.</param>
        /// <param name="scenario">The scenario</param>
        /// <returns></returns>
        ICollection<IAgentDayScheduleTag> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario);


        /// <summary>
        /// Finds the agent schedule day tag by date, person and scenario.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="person">The person.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        IAgentDayScheduleTag Find(DateOnly dateOnly, IPerson person, IScenario scenario);
    }
}
