﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonRequestRepository : IRepository<IPersonRequest>
	{
		/// <summary>
		/// Finds the person requests within the specified range.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-08-30
		/// </remarks>
		IList<IPersonRequest> Find(IPerson person, DateTimePeriod period);

		/// <summary>
		/// Finds all requests except those that are shift trades and have not been approved with the agent to trade with.
		/// </summary>
		/// <param name="persons">The list of agents to whom the shift trades belong (not the one trading with).</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Klas
		/// Created date: 2008-11-03
		/// </remarks>
		IList<IPersonRequest> FindAllExceptUnapprovedShiftTrades(ICollection<IPerson> persons);

		/// <summary>
		/// Finds all requests from and to a person
		/// </summary>
		/// <param name="person">The person.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-08-30
		/// </remarks>
		IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person);

		/// <summary>
		/// Finds all requests from and to a person for given page.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="paging">Paging information.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: mathiass
		/// Created date: 2011-10-24
		/// </remarks>
		IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, Paging paging);

		/// <summary>
		/// Finds all requests from and to a person for given period.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="period">The period to query.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: mathiass
		/// Created date: 2011-10-27
		/// </remarks>
		IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, DateTimePeriod period);

		IEnumerable<IPersonRequest> FindTextAndAbsenceRequestsForAgent(IPerson person, Paging paging);

		/// <summary>
		/// Finds all requests modified within or pending.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="period"></param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2009-10-29
		/// </remarks>
		IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(IPerson person, DateTimePeriod period);

		IList<IPersonRequest> FindPersonRequestUpdatedAfter(DateTime lastTime);

		/// <summary>
		/// Finds all requests modified within or pending.
		/// </summary>
		/// <param name="persons">The persons.</param>
		/// <param name="period"></param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2009-11-17
		/// </remarks>
		IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(ICollection<IPerson> persons, DateTimePeriod period);

		/// <summary>
		/// Finds request with the specified GUID.
		/// </summary>
		/// <param name="id">The GUID.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2010-03-24
		/// </remarks>
		IPersonRequest Find(Guid id);

	}
}
