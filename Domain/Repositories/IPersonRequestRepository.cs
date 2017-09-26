﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		/// Created by: rogerkr
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
		/// Created by: rogerkr
		/// Created date: 2011-10-27
		/// </remarks>
		IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, DateTimePeriod period);

		//IEnumerable<IPersonRequest> FindAbsenceAndTextRequests(RequestFilter filter);

		IEnumerable<IPersonRequest> FindAbsenceAndTextRequests(RequestFilter filter, out int count, bool ignoreCount = false);

		IEnumerable<IPersonRequest> FindShiftTradeRequests(RequestFilter filter, out int count, bool ignoreCount = false);

		IEnumerable<IPersonRequest> FindOvertimeRequests(RequestFilter filter, out int count, bool ignoreCount = false);

		/// <summary>
		/// Finds all specific types requests from and to a person for given page.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="paging">Paging information.</param>
		/// <param name="earliestDate">Hide requests earlier than this date</param>
		/// <param name="requestTypes">Paging information.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jianfeng
		/// Created date: 2015-07-22
		/// </remarks>
		IEnumerable<IPersonRequest> FindAllRequestsForAgentByType(IPerson person, Paging paging, DateTime? earliestDate, params RequestType[] requestTypes);		
		
		/// <summary>
		/// Finds all specific types requests from and to a person for given page, the find result will sort by requested date.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="paging">Paging information.</param>
		/// <param name="earliestDate">Hide requests earlier than this date</param>
		/// <param name="requestTypes">Paging information.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: mingdi
		/// Created date: 2016-11-04
		/// </remarks>
		IEnumerable<IPersonRequest> FindAllRequestsSortByRequestedDate(IPerson person, Paging paging, DateTime? earliestDate, params RequestType[] requestTypes);

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

		IList<IPersonRequest> Find(IEnumerable<Guid> id);

		IList<IPersonRequest> FindByStatus<T>(IPerson person, DateTime startDate, int status) where T: Request;

		IPersonRequest FindPersonRequestByRequestId(Guid value);

		IList<IShiftExchangeOffer> FindOfferByStatus(IPerson person, DateOnly date, ShiftExchangeOfferStatus status);

		IEnumerable<IShiftExchangeOffer> FindShiftExchangeOffersForBulletin(DateOnly shiftTradeDate);

		IList<Guid> GetWaitlistRequests(DateTimePeriod dateTimePeriod);

		IList<IPersonRequest> FindPersonRequestWithinPeriod(DateTimePeriod period);
		IEnumerable<DateTimePeriod> GetRequestPeriodsForAgent(IPerson person, DateTimePeriod period);
	}
}