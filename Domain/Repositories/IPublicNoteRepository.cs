using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPublicNoteRepository : IRepository<IPublicNote>, ILoadAggregateById<IPublicNote>, ILoadAggregateFromBroker<IPublicNote>
    {
        /// <summary>
        /// Finds all public notes for the specified period and scenario.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        IList<IPublicNote> Find(DateTimePeriod period, IScenario scenario);

        /// <summary>
        /// Finds all public notes for the specified period, persons and scenario.
        /// </summary>
        /// <param name="dateOnlyPeriod">The date only period.</param>
        /// <param name="personCollection">The person collection.</param>
        /// <param name="scenario">The scenario</param>
        /// <returns></returns>
        ICollection<IPublicNote> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario);


        /// <summary>
        /// Finds the public note specified by date, person and scenario.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <param name="person">The person.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        IPublicNote Find(DateOnly dateOnly, IPerson person, IScenario scenario);
    }
}
