using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	///  Interface for AgentAssignmentRepository
	/// </summary>
	public interface IPersonAssignmentRepository : IRepository<IPersonAssignment>, ILoadAggregateFromBroker<IPersonAssignment>
	{
		/// <summary>
		/// Finds the specified persons.
		/// </summary>
		/// <param name="persons">The persons.</param>
		/// <param name="period">The period.</param>
		/// <param name="scenario">The scenario.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Sumedah
		/// Created date: 2008-03-06
		/// </remarks>
		ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons,
						    DateOnlyPeriod period,
						    IScenario scenario);

		/// <summary>
		/// Finds the specified period.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="scenario">The scenario.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2007-11-12
		/// </remarks>
		ICollection<IPersonAssignment> Find(DateOnlyPeriod period, IScenario scenario);

		IEnumerable<DateScenarioPersonId> FetchDatabaseVersions(DateOnlyPeriod period, IScenario scenario, IPerson person);

		DateTime GetScheduleLoadedTime();
	}

	public class PersonAssignmentKey
	{
		public IScenario Scenario { get; set; }
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
	}
}