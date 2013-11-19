using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	///  Interface for AgentAssignmentRepository
	/// </summary>
	public interface IPersonAbsenceRepository : IRepository<IPersonAbsence>, IWriteSideRepository<IPersonAbsence>, ILoadAggregateFromBroker<IPersonAbsence>
	{
		/// <summary>
		/// Finds the specified Assignments.
		/// </summary>
		/// <param name="persons">The agents.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons, DateTimePeriod period);

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
		ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons,
						 DateTimePeriod period,
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
		ICollection<IPersonAbsence> Find(DateTimePeriod period, IScenario scenario);

		/// <summary>
		/// Finds the specified Assignments by the given Period param.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		ICollection<IPersonAbsence> Find(DateTimePeriod period);

		/// <summary>
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="scenario">The scenario.</param>
		/// <param name="period">The period.</param>
		/// <param name="absence">The absence.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-02-12
		/// </remarks>
		ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="persons"></param>
		/// <param name="optimizedPeriod"></param>
		/// <param name="scenario"></param>
		/// <param name="absence"></param>
		/// <returns></returns>
		IEnumerable<IPersonAbsence> Find(IList<IPerson> persons, DateTimePeriod optimizedPeriod, IScenario scenario, IAbsence absence);
	}
}