using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface MeetingRepository
    /// </summary>
    public interface IMeetingRepository : IRepository<IMeeting>, ILoadAggregateById<IMeeting>
    {
        
        /// <summary>
        /// Find by person, period, scenario
        /// </summary>
        /// <param name="persons"></param>
        /// <param name="period"></param>
        /// <param name="scenario"></param>
        /// <returns></returns>
        ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateTimePeriod period, IScenario scenario);

       /// <summary>
       /// Find by period, scenario
       /// </summary>
       /// <param name="period"></param>
       /// <param name="scenario"></param>
       /// <returns></returns>
       ICollection<IMeeting> Find(DateTimePeriod period, IScenario scenario);

       IList<IMeeting> FindMeetingsWithTheseOriginals(ICollection<IMeeting> meetings, IScenario scenario);

    	ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateTimePeriod period, IScenario scenario,
    	                           bool includeForOrganizer);

        ICollection<IMeeting> Find(IEnumerable<Guid> persons, DateTimePeriod period, IScenario scenario,
                                   bool includeForOrganizer);
    }
}
