using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Gets historical schedule data
	/// </summary>
	public interface IScheduleHistoryRepository
	{
		/// <summary>
		/// Returns the revisions where schedules has changed
		/// for an <paramref name="agent"/> at <paramref name="dateOnly"/>.
		/// Returned list is descending ordered, highest number first.
		/// Maximum number of items returned is defined by <paramref name="maxResult"/>,
		/// If more revisions exists, they will be discarded.
		/// </summary>
		/// <param name="agent">The agent to search for</param>
		/// <param name="dateOnly">The date to search for</param>
		/// <param name="maxResult">Max length of returned list</param>
		/// <returns></returns>
		IEnumerable<IRevision> FindRevisions(IPerson agent, DateOnly dateOnly, int maxResult);

		/// <summary>
		/// Returns schedule data for a specific <see cref="IRevision"/>
		/// for an <paramref name="agent"/> at <paramref name="dateOnly"/>.
		/// </summary>
		/// <param name="revision">The revision to search for</param>
		/// <param name="agent">The agent to search for</param>
		/// <param name="dateOnly">The date to search for</param>
		/// <returns></returns>
		IEnumerable<IPersistableScheduleData> FindSchedules(IRevision revision, IPerson agent, DateOnly dateOnly);
	}
}