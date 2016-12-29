﻿using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Collection;
using NHibernate;
using System.Linq;
using NHibernate.Dialect.Function;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for person requests
	/// </summary>
	public class PersonRequestRepository : Repository<IPersonRequest>, IPersonRequestRepository
	{
		private const char splitter = ' ';

		public PersonRequestRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public PersonRequestRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IList<IPersonRequest> FindByStatus<T>(IPerson person, DateTime startDateTime, int status) where T : Request
		{
			var requestForPeriod = createRequestForPeriodCriteria(startDateTime).Add(Restrictions.Eq("class", typeof(T)));
			return findRequestsByRequestPeriod(person, requestForPeriod, status);
		}

		public IList<IShiftExchangeOffer> FindOfferByStatus(IPerson person, DateOnly date, ShiftExchangeOfferStatus status)
		{
			return Session.CreateCriteria<IShiftExchangeOffer>()
				.Add(Restrictions.Eq("Date", date))
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("Status", status))
				.List<IShiftExchangeOffer>();
		}

		public IPersonRequest FindPersonRequestByRequestId(Guid id)
		{
			return Session.GetNamedQuery("findPersonRequestByRequestId")
				.SetGuid("Id", id)
				.UniqueResult<IPersonRequest>();
		}

		public IList<IPersonRequest> Find(IPerson person, DateTimePeriod period)
		{
			var requestForPeriod = createRequestForPeriodCriteria(period);
			return findRequestsByRequestPeriod(person, requestForPeriod);
		}

		private static DetachedCriteria createRequestForPeriodCriteria(DateTimePeriod period)
		{
			var requestForPeriod = DetachedCriteria.For<Request>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Ge("Period.period.Maximum", period.StartDateTime))
				.Add(Restrictions.Le("Period.period.Minimum", period.EndDateTime));
			return requestForPeriod;
		}

		private static DetachedCriteria createRequestForPeriodCriteria(DateTime startDateTime)
		{
			var requestForPeriod = DetachedCriteria.For<Request>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Eq("Period.period.Minimum", startDateTime))
				.Add(Restrictions.Not(Restrictions.Eq("class", typeof(ShiftTradeRequest))))
				.Add(Restrictions.Not(Restrictions.Eq("class", typeof(AbsenceRequest))))
				.Add(Restrictions.Not(Restrictions.Eq("class", typeof(TextRequest))));
			return requestForPeriod;
		}

		private IList<IPersonRequest> findRequestsByRequestPeriod(IPerson person, DetachedCriteria requestForPeriod,
			int status = 0)
		{
			return Session.CreateCriteria<IPersonRequest>("req")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("requestStatus", status))
				.SetFetchMode("requests", FetchMode.Join)
				.Add(Subqueries.PropertyIn("Id", requestForPeriod))
				.List<IPersonRequest>();
		}

		public IPersonRequest Find(Guid id)
		{
			var returnPersonRequest = Session.CreateCriteria(typeof(PersonRequest), "req")
				.Add(Restrictions.Eq("Id", id))
				.SetFetchMode("requests", FetchMode.Join)
				.SetFetchMode("Person", FetchMode.Join)
				.UniqueResult<IPersonRequest>();

			var shiftTrade = returnPersonRequest?.Request as IShiftTradeRequest;

			if (shiftTrade != null)
			{
				LazyLoadingManager.Initialize(shiftTrade.ShiftTradeSwapDetails);
			}

			return returnPersonRequest;
		}

		public IList<IPersonRequest> Find(IEnumerable<Guid> ids)
		{
			var returnPersonRequests = new List<IPersonRequest>();
			foreach (var idBatch in ids.Batch(1000))
			{
				returnPersonRequests.AddRange(Session.CreateCriteria(typeof(PersonRequest), "req")
					.Add(Restrictions.In("Id", idBatch.ToArray()))
					.SetFetchMode("requests", FetchMode.Join)
					.List<IPersonRequest>());
			}

			foreach (var returnPersonRequest in returnPersonRequests)
			{
				var absenceRequest = returnPersonRequest.Request as IAbsenceRequest;
				if (absenceRequest != null)
				{
					LazyLoadingManager.Initialize(absenceRequest.Absence);
				}
			}
			return returnPersonRequests;
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, Paging paging)
		{
			var personRequests = getAllRequests(person);

			applyPagingToResults(paging, personRequests);

			return personRequests.List<IPersonRequest>();
		}

		public IEnumerable<IPersonRequest> FindAllRequests(RequestFilter filter)
		{
			int count;
			return FindAllRequests(filter, out count, true);
		}

		public IEnumerable<IPersonRequest> FindAllRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			var criteria = Session.CreateCriteria<IPersonRequest>("personRequests");
			criteria.SetFetchMode("requests", FetchMode.Join);
			criteria.CreateCriteria("Person", "p", JoinType.InnerJoin);
			criteria.CreateCriteria("requests", "req", JoinType.InnerJoin);

			filterRequestByPeriod(criteria, filter);
			filterRequestByPersons(criteria, filter.Persons);
			filterRequestBelongsToUndeleted(criteria);
			filterRequestByRequestType(criteria, filter.RequestTypes);
			filterRequestByRequestFilters(criteria, filter.RequestFilters);
			filterRequestByShiftTradeStatus(criteria, filter);
			if (ignoreCount)
			{
				count = -1;
			}
			else
			{
				var criteriaCount = (ICriteria)criteria.Clone();
				criteriaCount.SetProjection(Projections.RowCount());
				count = Convert.ToInt32(criteriaCount.UniqueResult());
			}

			sortPersonRequests(criteria, filter.SortingOrders);
			pagePersonRequests(criteria, filter.Paging);

			return criteria.List<IPersonRequest>();
		}

		private static void filterRequestByPeriod(ICriteria criteria, RequestFilter filter)
		{
			var period = filter.Period;

			var requestForPeriod = DetachedCriteria.For<Request>()
				.SetProjection(Projections.Property("Parent"));

			if (filter.ExcludeRequestsOnFilterPeriodEdge)
			{
				requestForPeriod
					.Add(Restrictions.Gt("Period.period.Maximum", period.StartDateTime))
					.Add(Restrictions.Lt("Period.period.Minimum", period.EndDateTime));
			}
			else
			{
				requestForPeriod
					.Add(Restrictions.Ge("Period.period.Maximum", period.StartDateTime))
					.Add(Restrictions.Le("Period.period.Minimum", period.EndDateTime));
			}

			if (filter.OnlyIncludeRequestsStartingWithinPeriod)
			{
				requestForPeriod.Add(Restrictions.Ge("Period.period.Minimum", period.StartDateTime));
			}

			criteria.Add(Subqueries.PropertyIn("Id", requestForPeriod));
		}

		private void filterRequestBelongsToUndeleted(ICriteria criteria)
		{
			criteria.Add(Restrictions.Eq("p.IsDeleted", false));
		}

		private void filterRequestByPersons(ICriteria criteria, IEnumerable<IPerson> persons)
		{
			if (persons == null) return;

			var people = persons.ToArray();
	
			var personIn = createPersonInCriterion("Person", people);
			var personInShiftTradeTo = includeRequestsWithShiftTradePersonTo(people);

			criteria.Add(Restrictions.Or(personInShiftTradeTo, personIn));
		}

		private static AbstractCriterion includeRequestsWithShiftTradePersonTo(IPerson[] people)
		{
			var shiftTradeDetailsForAgentPersonTo = DetachedCriteria.For<ShiftTradeSwapDetail>()
				.SetProjection(Projections.Property("Parent"))
				.Add(createPersonInCriterion("PersonTo", people));

			var shiftTradeRequestsForAgentPersonTo = DetachedCriteria.For<ShiftTradeRequest>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Subqueries.PropertyIn("ShiftTradeSwapDetails", shiftTradeDetailsForAgentPersonTo));

			return Subqueries.PropertyIn("requests", shiftTradeRequestsForAgentPersonTo);
		}

		private void filterRequestByRequestType(ICriteria criteria, IEnumerable<RequestType> requestTypes)
		{
			if (requestTypes == null) return;
			var typeCriterion = requestTypes.Select(toRequestClassTypeConstraint)
				.Where(c => c != null).Aggregate(Restrictions.Or);
			criteria.Add(typeCriterion);
		}

		private void filterRequestByRequestFilters(ICriteria criteria,
			IDictionary<RequestFilterField, string> requestFilters)
		{
			if (requestFilters == null || !requestFilters.Any()) return;

			foreach (var filter in requestFilters)
			{
				switch (filter.Key)
				{
					case RequestFilterField.Status:
						addStatusCriteria(criteria, filter);
						break;

					case RequestFilterField.Subject:
						var subjectCriterion = getStringFilterCriteria(filter.Value, "personRequests.Subject");
						if (subjectCriterion != null) criteria.Add(subjectCriterion);
						break;

					case RequestFilterField.Message:
						var messageCriterion = getStringFilterCriteria(filter.Value, "personRequests.Message");
						if (messageCriterion != null) criteria.Add(messageCriterion);
						break;

					case RequestFilterField.Type:
						addTypeCriterion(criteria, filter);
						break;

					default:
						continue;
				}
			}
		}

		private void addTypeCriterion(ICriteria criteria, KeyValuePair<RequestFilterField, string> filter)
		{
			var filterTextRequest = false;

			var absenceFilters = filter.Value.Split(splitter).Select(x =>
			{
				RequestType requestType;
				if (Enum.TryParse(x, out requestType))
				{
					filterTextRequest = requestType == RequestType.TextRequest;
					return Guid.Empty;
				}
				Guid absenceId;
				return Guid.TryParse(x.Trim(), out absenceId) ? absenceId : Guid.Empty;
			}).Where(x => x != Guid.Empty).ToList();

			if (!absenceFilters.Any() && !filterTextRequest) return;

			var filterAbsenceRequest = absenceFilters.Any();
			ICriterion absenceRequestCriterion = null;

			if (filterAbsenceRequest)
			{
				var absenceCriterion = absenceFilters
					.Select(x => (ICriterion)Restrictions.Eq("req.Absence.Id", x))
					.Aggregate(Restrictions.Or);

				absenceRequestCriterion = Restrictions.And(absenceCriterion,
					toRequestClassTypeConstraint(RequestType.AbsenceRequest));

				if (!filterTextRequest)
				{
					criteria.Add(absenceRequestCriterion);
					return;
				}
			}

			if (filterTextRequest)
			{
				var textRequestTypeCriterion = toRequestClassTypeConstraint(RequestType.TextRequest);

				if (!filterAbsenceRequest)
				{
					criteria.Add(textRequestTypeCriterion);
					return;
				}

				criteria.Add(Restrictions.Or(absenceRequestCriterion, textRequestTypeCriterion));
			}
		}

		private void filterRequestByShiftTradeStatus(ICriteria criteria, RequestFilter filter)
		{
			if (!filter.ExcludeInvalidShiftTradeRequest) return;

			criteria.Add(toRequestClassTypeConstraint(RequestType.ShiftTradeRequest));
			criteria.Add(!Restrictions.Eq("req.shiftTradeStatus", ShiftTradeStatus.OkByMe));
			criteria.Add(!Restrictions.Eq("req.shiftTradeStatus", ShiftTradeStatus.Referred));
		}

		private static void addStatusCriteria(ICriteria criteria, KeyValuePair<RequestFilterField, string> filter)
		{
			var statusFilters = filter.Value.Split(splitter).Select(x =>
			{
				int status;
				return int.TryParse(x.Trim(), out status) ? status : int.MinValue;
			}).Where(x => x > int.MinValue).ToList();

			if (!statusFilters.Any()) return;

			addAutoDeniedFilterIfDeniedFilterIsIncluded(statusFilters);

			var statusCriterion = statusFilters
				.Select(x => (ICriterion) Restrictions.Eq("personRequests.requestStatus", x))
				.Aggregate(Restrictions.Or);
			criteria.Add(statusCriterion);
		}

		private static void addAutoDeniedFilterIfDeniedFilterIsIncluded(ICollection<int> statusFilters)
		{
			if (statusFilters.Contains((int) PersonRequestStatus.Denied))
			{
				statusFilters.Add((int) PersonRequestStatus.AutoDenied);
			}
		}

		private static ICriterion getStringFilterCriteria(string filterKeywords, string propertyPath)
		{
			var subjectKeywords = filterKeywords.Split(splitter)
				.Select(x => x.Trim().ToLower())
				.Where(x => !string.IsNullOrEmpty(x)).ToList();

			if (!subjectKeywords.Any()) return null;

			var criteria = subjectKeywords
				.Select(x => (ICriterion) Restrictions.Like(propertyPath, $"%{x}%"))
				.Aggregate(Restrictions.And);
			return criteria;
		}

		private ICriterion toRequestClassTypeConstraint(RequestType t)
		{
			Type type;
			switch (t)
			{
				case RequestType.AbsenceRequest:
					type = typeof(AbsenceRequest);
					break;

				case RequestType.TextRequest:
					type = typeof(TextRequest);
					break;

				case RequestType.ShiftTradeRequest:
					type = typeof(ShiftTradeRequest);
					break;

				default:
					return null;
			}
			return Restrictions.Eq("req.class", type);
		}

		private void pagePersonRequests(ICriteria query, Paging paging)
		{
			if (paging.Equals(Paging.Empty)) return;
			query.SetFirstResult(paging.Skip).SetMaxResults(paging.Take);
		}

		private IEnumerable<IPersonRequest> filteredRequests(IPerson person, DateTime? earliestDate, params RequestType[] requestTypes)
		{
			var allRequests = FindAllRequestsForAgent(person);
			var filteredRequests = allRequests.Where(x => requestTypes.Contains(x.Request.RequestType));
			if (earliestDate != null)
			{
				filteredRequests = filteredRequests.Where(x=>x.Request.Period.StartDateTime >= earliestDate);
			}

			return filteredRequests;
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgentByType(IPerson person, Paging paging, DateTime? earliestDate, params RequestType[] requestTypes)
		{
			var requests = filteredRequests(person, earliestDate, requestTypes).OrderByDescending(x => x.UpdatedOn);

			return paging.Equals(Paging.Empty) ? requests : requests.Skip(paging.Skip).Take(paging.Take);
		}


		public IEnumerable<IPersonRequest> FindAllRequestsSortByRequestedDate(IPerson person, Paging paging, DateTime? earliestDate, params RequestType[] requestTypes)
		{
			var requests = filteredRequests(person, earliestDate, requestTypes).OrderByDescending(x => x.RequestedDate);

			return paging.Equals(Paging.Empty) ? requests : requests.Skip(paging.Skip).Take(paging.Take);
		}

		private static AbstractCriterion getShiftTradeRequestsForAgent(IPerson person)
		{
			var shiftTradeDetailsForAgentPersonTo = DetachedCriteria.For<ShiftTradeSwapDetail>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Eq("PersonTo", person));

			var shiftTradeDetailsForAgentPersonFrom = DetachedCriteria.For<ShiftTradeSwapDetail>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Eq("PersonFrom", person));

			var shiftTradeRequestsForAgentPersonTo = DetachedCriteria.For<ShiftTradeRequest>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Subqueries.PropertyIn("ShiftTradeSwapDetails", shiftTradeDetailsForAgentPersonTo));

			var shiftTradeRequestsForAgentPersonFrom = DetachedCriteria.For<ShiftTradeRequest>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Subqueries.PropertyIn("ShiftTradeSwapDetails", shiftTradeDetailsForAgentPersonFrom));

			var requestsForAgent = Restrictions.Or(Restrictions.Or(
				Restrictions.And(Subqueries.PropertyIn("requests", shiftTradeRequestsForAgentPersonTo),
					Restrictions.Not(Restrictions.Eq("requestStatus", 4))), // hide auto denied shift trade requests for receiptors
				Subqueries.PropertyIn("requests", shiftTradeRequestsForAgentPersonFrom)),
				Restrictions.Eq("Person", person));

			return requestsForAgent;
		}

		private ICriteria getAllRequests(IPerson person)
		{
			var personRequests = Session.CreateCriteria<PersonRequest>()
				.SetFetchMode("requests", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.AddOrder(Order.Desc("UpdatedOn"));
			var requestsForAgent = getShiftTradeRequestsForAgent(person);

			personRequests.Add(requestsForAgent);
			return personRequests;
		}

		private static void applyPagingToResults(Paging paging, ICriteria personRequests)
		{
			if (paging.Equals(Paging.Empty)) return;

			personRequests.SetMaxResults(paging.Take);
			personRequests.SetFirstResult(paging.Skip);
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person)
		{
			return findAllRequestsForAgent(person, null);
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, DateTimePeriod period)
		{
			return findAllRequestsForAgent(person, period);
		}

		private IEnumerable<IPersonRequest> findAllRequestsForAgent(IPerson person, DateTimePeriod? period)
		{
			var requestsCreatedByPerson = getRequestsCreatedByAgent(person, period).List<IPersonRequest>();
			var shiftTradeRequestsWithPerson = getShiftTradeRequestsWithAgent(person, period).List<IPersonRequest>();

			return requestsCreatedByPerson.Union(shiftTradeRequestsWithPerson).OrderByDescending(x => x.UpdatedOn);
		}

		/// <summary>
		/// Get all requests created by agent
		/// </summary>
		private ICriteria getRequestsCreatedByAgent(IPerson person, DateTimePeriod? period)
		{
			var criteriaPersonRequestsCreatedByPerson = Session.CreateCriteria<PersonRequest>()
				.SetFetchMode("requests", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.Add(Restrictions.Eq("Person", person));

			applyPeriodRestriction(criteriaPersonRequestsCreatedByPerson, period);

			return criteriaPersonRequestsCreatedByPerson;
		}

		/// <summary>
		/// Get shift trade request created by other agent but trade with current person
		/// </summary>
		private ICriteria getShiftTradeRequestsWithAgent(IPerson person, DateTimePeriod? period)
		{
			var subQueryShiftTradeRequestsWithPerson = DetachedCriteria.For<ShiftTradeRequest>()
				.SetProjection(Projections.Property("Parent"))
				.CreateCriteria("ShiftTradeSwapDetails", "swapDetail", JoinType.InnerJoin)
				.Add(Restrictions.Eq("swapDetail.PersonTo", person));

			var criteriaShiftTradeRequestsWithPerson = Session.CreateCriteria<PersonRequest>()
				.SetFetchMode("requests", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.Add(Restrictions.Not(Restrictions.Eq("requestStatus", 4)))
				.Add(Subqueries.PropertyIn("requests", subQueryShiftTradeRequestsWithPerson));

			applyPeriodRestriction(criteriaShiftTradeRequestsWithPerson, period);

			return criteriaShiftTradeRequestsWithPerson;
		}

		private void applyPeriodRestriction(ICriteria criteria, DateTimePeriod? period)
		{
			if (period == null) return;

			criteria.CreateCriteria("requests", JoinType.LeftOuterJoin)
				.Add(Restrictions.Ge("Period.period.Maximum", period.Value.StartDateTime))
				.Add(Restrictions.Le("Period.period.Minimum", period.Value.EndDateTime));
		}

		/// <summary>
		/// Finds all request within period or pending.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-10-29
		/// </remarks>
		public IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(IPerson person, DateTimePeriod period)
		{
			//Alla request där UpdatedOn innanför period
			var allRequestsBy = findModifiedWithinPeriodOrPending(person, period);

			//Alla shiftTrades där UpdatedOn innanför Period
			var allShiftTradesTo = findShiftTradesModifiedWithinPeriod(person, period);

			return allRequestsBy.Union(allShiftTradesTo).ToList();
		}

		private IEnumerable<IPersonRequest> findShiftTradesModifiedWithinPeriod(IPerson person, DateTimePeriod period)
		{
			var personFrom = Subqueries.PropertyIn("requests",
				DetachedCriteria.For(typeof(ShiftTradeRequest))
					.SetProjection(Projections.Property("Parent"))
					.Add(Subqueries.PropertyIn("ShiftTradeSwapDetails",
						DetachedCriteria.For<ShiftTradeSwapDetail>()
							.SetProjection(Projections.Property("Parent"))
							.Add(Restrictions.Eq("PersonFrom", person)))));
			var personTo = Subqueries.PropertyIn("requests",
				DetachedCriteria.For(typeof(ShiftTradeRequest))
					.SetProjection(Projections.Property("Parent"))
					.Add(Subqueries.PropertyIn("ShiftTradeSwapDetails",
						DetachedCriteria.For<ShiftTradeSwapDetail>()
							.SetProjection(Projections.Property("Parent"))
							.Add(Restrictions.Eq("PersonTo", person)))));

			var personFromRestriction =
				Restrictions.And(
					Restrictions.Or(Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
						Restrictions.Eq("requestStatus", 0)), personFrom);
			var personToRestriction =
				Restrictions.And(
					Restrictions.Or(
						Restrictions.And(Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
							Restrictions.Not(Restrictions.Eq("requestStatus", 4))), Restrictions.Eq("requestStatus", 0)),
					personTo);

			var retList = Session.CreateCriteria(typeof(PersonRequest))
				.Add(Restrictions.Or(personToRestriction, personFromRestriction))
				.SetFetchMode("requests", FetchMode.Join)
				.List<IPersonRequest>();

			return retList;
		}

		private IEnumerable<IPersonRequest> findModifiedWithinPeriodOrPending(IPerson person, DateTimePeriod period)
		{
			var retList = Session.CreateCriteria(typeof(PersonRequest))
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Or(Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
					Restrictions.Eq("requestStatus", 0)))
				.SetFetchMode("requests", FetchMode.Join)
				.List<IPersonRequest>();
			return retList;
		}

		public IList<IPersonRequest> FindPersonRequestWithinPeriod(DateTimePeriod period)
		{
			var allRequestExceptShiftTrade = findPersonRequestWithinPeriodExceptShiftTrade(period);
			var allShiftTradeRequests = findShiftTradeRequestWithinPeriod(period);

			return allRequestExceptShiftTrade.Union(allShiftTradeRequests).ToList();
		}

		public IList<IPersonRequest> FindRequestsForDate(DateTime dateTime)
		{
			var date = dateTime.Date;
			return Session.CreateCriteria<IPersonRequest>()
				.Add(Restrictions.Between("CreatedOn", date, date.AddDays(1)))
				.List<IPersonRequest>();
		}

		private IEnumerable<IPersonRequest> findPersonRequestWithinPeriodExceptShiftTrade(DateTimePeriod period)
		{
			var requestForPeriod = DetachedCriteria.For<Request>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Between("Period.period.Minimum", period.StartDateTime, period.EndDateTime))
				.Add(Restrictions.Not(Restrictions.Eq("class", typeof(ShiftTradeRequest))));

			return Session.CreateCriteria<IPersonRequest>()
				.Add(Restrictions.Not(Restrictions.Eq("requestStatus", 3)))
				.SetFetchMode("requests", FetchMode.Join)
				.SetFetchMode("Person", FetchMode.Join)
				.Add(Subqueries.PropertyIn("Id", requestForPeriod))
				.List<IPersonRequest>();
		}

		private IEnumerable<IPersonRequest> findShiftTradeRequestWithinPeriod(DateTimePeriod period)
		{
			var requestForPeriod = DetachedCriteria.For<ShiftTradeRequest>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Between("Period.period.Minimum", period.StartDateTime, period.EndDateTime));

			return Session.CreateCriteria<IPersonRequest>()
				.Add(Restrictions.Not(Restrictions.Eq("requestStatus", 3)))
				.SetFetchMode("requests", FetchMode.Join)
				.SetFetchMode("Person", FetchMode.Join)
				.SetFetchMode("requests.ShiftTradeSwapDetails", FetchMode.Join)
				.Add(Subqueries.PropertyIn("Id", requestForPeriod))
				.List<IPersonRequest>();
		}

		/// <summary>
		/// Finds all requests within a period for a list of persons and ALL request that are pending for the persons in the list
		/// </summary>
		/// <param name="persons"></param>
		/// <param name="period"></param>
		/// <returns></returns>
		public IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(ICollection<IPerson> persons,
			DateTimePeriod period)
		{
			//Alla request där UpdatedOn innanför period
			var allRequestsBy = findModifiedWithinPeriodOrPendingExceptShiftTrades(persons, period);

			//Alla shiftTrades där UpdatedOn innanför Period
			var allShiftTradesTo = findShiftTradesModifiedWithinPeriod(persons, period);

			return allRequestsBy.Union(allShiftTradesTo).ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private IEnumerable<IPersonRequest> findShiftTradesModifiedWithinPeriod(IEnumerable<IPerson> persons,
			DateTimePeriod period)
		{
			// parameter limit is 2100, and the person id list is passed twice
			const int personChunkSize = 1000;

			var personChunks = persons.Batch(personChunkSize);

			var personRequestResults =
				from personChunk in personChunks
				let personChunkList = personChunk.ToArray()
				select
					Session.CreateCriteria(typeof(PersonRequest))
						.SetFetchMode("requests", FetchMode.Join)
						.SetFetchMode("requests.ShiftTradeSwapDetails", FetchMode.Join)
						.Add
						(
							Restrictions.Or(
								Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
								Restrictions.Eq("requestStatus", 0))
						)
						.Add(Subqueries.PropertyIn("requests", DetachedCriteria.For(typeof(ShiftTradeRequest))
							.SetProjection(Projections.Property("Parent"))
							.Add(Subqueries.PropertyIn("ShiftTradeSwapDetails",
								DetachedCriteria.For<ShiftTradeSwapDetail>().SetProjection(Projections.Property("Parent"))
									.CreateAlias("PersonTo", "personTo", JoinType.InnerJoin)
									.CreateAlias("PersonFrom", "personFrom", JoinType.InnerJoin)
									.Add(Restrictions.Eq("personTo.IsDeleted", false))
									.Add(Restrictions.Eq("personFrom.IsDeleted", false))
									.Add(Restrictions.Or(Restrictions.IsNull("personTo.TerminalDate"),
										Restrictions.Gt("personTo.TerminalDate", DateOnly.Today)))
									.Add(Restrictions.Or(Restrictions.IsNull("personFrom.TerminalDate"),
										Restrictions.Gt("personFrom.TerminalDate", DateOnly.Today)))
									.Add(Restrictions.InG("PersonFrom", personChunkList))
									.Add(Restrictions.InG("PersonTo", personChunkList))))))
						.List<IPersonRequest>();

			var personRequests =
				(from result in personRequestResults
					from request in result
					select request).Distinct();

			var personRequestList = personRequests.ToList();

			personRequestList.ForEach(x => LazyLoadingManager.Initialize(x.Request));
			personRequestList.ForEach(x => LazyLoadingManager.Initialize(x.Request.Person));
			personRequestList.ForEach(x => LazyLoadingManager.Initialize(x.Request.Person.PersonPeriodCollection));
			personRequestList.ForEach(x => LazyLoadingManager.Initialize(x.Request.PersonFrom));
			personRequestList.ForEach(x => LazyLoadingManager.Initialize(x.Request.PersonFrom.PersonPeriodCollection));
			personRequestList.ForEach(x => LazyLoadingManager.Initialize(x.Request.PersonTo));
			personRequestList.ForEach(x => LazyLoadingManager.Initialize(x.Request.PersonTo.PersonPeriodCollection));

			return personRequestList;
		}

		private IEnumerable<IPersonRequest> findModifiedWithinPeriodOrPendingExceptShiftTrades(IEnumerable<IPerson> persons,
			DateTimePeriod period)
		{
			var retList = new List<IPersonRequest>();

			foreach (var item in persons.Batch(2000))
			{
				retList.AddRange(Session.CreateCriteria(typeof(PersonRequest))
					.SetFetchMode("requests", FetchMode.Join)
					.SetFetchMode("requests.ShiftTradeSwapDetails", FetchMode.Join)
					.Add(Restrictions.InG("Person", item.ToArray()))
					.Add(Restrictions.Or(Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
						Restrictions.Eq("requestStatus", 0)))
					.List<IPersonRequest>());
			}

			//sjukt korkat - men finns inget lämpligt att filtrera p?i frågan ovan (varför en lista av requests!?)
			retList = retList.Where(pr => !(pr.Request is IShiftTradeRequest)).ToList();

			foreach (var personRequest in retList)
			{
				LazyLoadingManager.Initialize(personRequest.Person);
				LazyLoadingManager.Initialize(personRequest.Person.PersonPeriodCollection);
			}

			return retList;
		}

		public override void Remove(IPersonRequest entity)
		{
			if (entity.IsDenied && (!entity.IsWaitlisted))
				throw new DataSourceException("Cannot delete person request " + entity + ". It has already been denied.");
			if (entity.IsApproved)
				throw new DataSourceException("Cannot delete person request " + entity + ". It has already been approved.");
			base.Remove(entity);
		}

		public IEnumerable<IShiftExchangeOffer> FindShiftExchangeOffersForBulletin(IEnumerable<IPerson> personList,
			DateOnly shiftTradeDate)
		{
			return Session.CreateCriteria(typeof(IShiftExchangeOffer), "offer")
				.CreateCriteria("Parent", "req", JoinType.InnerJoin)
				.Add(Restrictions.Eq("req.IsDeleted", false))
				.Add(Restrictions.Eq("offer.Date", shiftTradeDate))
				.Add(Restrictions.InG("offer.Person", personList.ToArray()))
				.Add(Restrictions.Ge("offer.Criteria.ValidTo", new DateOnly(DateTime.UtcNow.Date)))
				.Add(Restrictions.Eq("offer.Status", ShiftExchangeOfferStatus.Pending))
				.List<ShiftExchangeOffer>();
		}

		private static void sortPersonRequests(ICriteria criteria, IList<RequestsSortingOrder> sortingOrders)
		{
			if (sortingOrders == null) return;

			var orderMapping = new Dictionary<RequestsSortingOrder, Action<ICriteria>>
			{
				{
					RequestsSortingOrder.AgentNameAsc, c =>
					{
						c.AddOrder(Order.Asc("p.Name.LastName"));
						c.AddOrder(Order.Asc("p.Name.FirstName"));
					}
				},
				{
					RequestsSortingOrder.AgentNameDesc, c =>
					{
						c.AddOrder(Order.Desc("p.Name.LastName"));
						c.AddOrder(Order.Desc("p.Name.FirstName"));
					}
				},
				{RequestsSortingOrder.SubjectAsc, c => c.AddOrder(Order.Asc("personRequests.Subject"))},
				{RequestsSortingOrder.SubjectDesc, c => c.AddOrder(Order.Desc("personRequests.Subject"))},
				{RequestsSortingOrder.CreatedOnAsc, c => c.AddOrder(Order.Asc("personRequests.CreatedOn"))},
				{RequestsSortingOrder.CreatedOnDesc, c => c.AddOrder(Order.Desc("personRequests.CreatedOn"))},
				{RequestsSortingOrder.DenyReasonAsc, c => c.AddOrder(Order.Asc("personRequests.DenyReason"))},
				{RequestsSortingOrder.DenyReasonDesc, c => c.AddOrder(Order.Desc("personRequests.DenyReason"))},
				{RequestsSortingOrder.MessageAsc, c => c.AddOrder(Order.Asc("personRequests.Message"))},
				{RequestsSortingOrder.MessageDesc, c => c.AddOrder(Order.Desc("personRequests.Message"))},

				{RequestsSortingOrder.UpdatedOnAsc, c => c.AddOrder(Order.Asc("personRequests.UpdatedOn"))},
				{RequestsSortingOrder.UpdatedOnDesc, c => c.AddOrder(Order.Desc("personRequests.UpdatedOn"))},
				{RequestsSortingOrder.PeriodStartAsc, c => c.AddOrder(Order.Asc("req.Period.period.Minimum"))},
				{RequestsSortingOrder.PeriodStartDesc, c => c.AddOrder(Order.Desc("req.Period.period.Minimum"))},
				{RequestsSortingOrder.PeriodEndAsc, c => c.AddOrder(Order.Asc("req.Period.period.Maximum"))},
				{RequestsSortingOrder.PeriodEndDesc, c => c.AddOrder(Order.Desc("req.Period.period.Maximum"))},

				{RequestsSortingOrder.SeniorityAsc, c => c.AddOrder(Order.Asc(getSeniorityProjection()))},
				{RequestsSortingOrder.SeniorityDesc, c => c.AddOrder(Order.Desc(getSeniorityProjection()))},

				{RequestsSortingOrder.TeamAsc, c => c.AddOrder(Order.Asc(getTeamProjection()))},
				{RequestsSortingOrder.TeamDesc, c => c.AddOrder(Order.Desc(getTeamProjection()))},
			};

			foreach (var order in sortingOrders)
			{
				Action<ICriteria> orderAction;
				if (orderMapping.TryGetValue(order, out orderAction))
				{
					orderAction(criteria);
				}
			}
		}

		private static IProjection getSeniorityProjection()
		{
			var dateDiffFunction = Projections.SqlFunction(
				new VarArgsSQLFunction("DateDiff(DAY,", ",", ")+1"),
				NHibernateUtil.Int32,
				Projections.Property<PersonPeriod>(val => val.StartDate),
				Projections.Conditional(
					Restrictions.Where<PersonPeriod>(period => period.internalEndDate >= DateOnly.Today),
					Projections.Constant(DateTime.Today, NHibernateUtil.DateTime),
					Projections.Cast(NHibernateUtil.DateTime, Projections.Property<PersonPeriod>(val => val.internalEndDate))));

			var senioritySubquery = DetachedCriteria.For<PersonPeriod>("pp")
				.Add(Restrictions.EqProperty("pp.Parent", "p.Id"))
				.Add(Restrictions.Le("StartDate", DateOnly.Today))
				.SetProjection(Projections.Sum(dateDiffFunction));

			return Projections.SubQuery(senioritySubquery);
		}

		private static IProjection getTeamProjection()
		{
			var team = DetachedCriteria.For<Team>("team")
				.Add(Restrictions.EqProperty("team.Id", "pp.Team"))
				.SetProjection(Projections.Property("Description.Name"));

			var personPeriodSubquery = DetachedCriteria.For<PersonPeriod>("pp")
				.Add(Restrictions.EqProperty("pp.Parent", "p.Id"))
				.Add(Restrictions.LeProperty("StartDate", "req.Period.period.Minimum"))
				.Add(Restrictions.GeProperty("internalEndDate", "req.Period.period.Minimum"))
				.SetProjection(Projections.SubQuery(team));

			return Projections.SubQuery(personPeriodSubquery);
		}

		public IList<Guid> GetWaitlistRequests(DateTimePeriod dateTimePeriod)
		{
			return  Session.CreateSQLQuery(
				  "SELECT DISTINCT pr.Id from PersonRequest pr, request r  where " +
				  "	pr.id = r.parent and" +
				  "	RequestStatus = 5 and " +
				  "	(r.startDateTime <= :endDate and r.endDateTime >= :startDate)" )
										  .SetDateTime("startDate", dateTimePeriod.StartDateTime)
										  .SetDateTime("endDate", dateTimePeriod.EndDateTime)
										  .List<Guid>();
		}

		private static AbstractCriterion createPersonInCriterion(string propertyName, IReadOnlyCollection<IPerson> people)
		{
			if (people.Count <= 1000)
			{
				return Restrictions.InG(propertyName, people);
			}

			return new MoreParameterInCriterion(Projections.Property(propertyName),
				people.Select(p => p.Id.GetValueOrDefault()).ToArray());
		}
	}
}