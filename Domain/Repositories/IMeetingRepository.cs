using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	/// <summary>
	/// Interface MeetingRepository
	/// </summary>
	public interface IMeetingRepository : IRepository<IMeeting>, ILoadAggregateById<IMeeting>, ILoadAggregateFromBroker<IMeeting>
	{
		/// <summary>
		/// Find by period, scenario
		/// </summary>
		/// <param name="period"></param>
		/// <param name="scenario"></param>
		/// <returns></returns>
		ICollection<IMeeting> Find(DateTimePeriod period, IScenario scenario);

		IList<IMeeting> FindMeetingsWithTheseOriginals(ICollection<IMeeting> meetings, IScenario scenario);

		ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario,
					   bool includeForOrganizer = true);
	}
}
