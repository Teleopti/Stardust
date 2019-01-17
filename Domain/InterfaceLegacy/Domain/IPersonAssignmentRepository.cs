using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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


		IEnumerable<DateScenarioPersonId> FetchDatabaseVersions(DateOnlyPeriod period, IScenario scenario, IPerson person);

		DateTime GetScheduleLoadedTime();

		ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons,
			DateOnlyPeriod period,
			IScenario scenario,
			string source);

		bool IsThereScheduledAgents(Guid businessUnitId, DateOnlyPeriod period);
	}

	public class PersonAssignmentKey
	{
		public IScenario Scenario { get; set; }
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
	}
}