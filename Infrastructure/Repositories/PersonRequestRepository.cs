using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Collection;
using NHibernate;
using System.Linq;
using NHibernate.Dialect.Function;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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
		
		private IList<IPersonRequest> findRequestsByRequestPeriod(IPerson person, DetachedCriteria requestForPeriod,
			int status = 0)
		{
			return Session.CreateCriteria<IPersonRequest>("req")
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Eq("requestStatus", status))
				.Fetch("requests")
				.Add(Subqueries.PropertyIn("Id", requestForPeriod))
				.List<IPersonRequest>();
		}

		public IPersonRequest Find(Guid id)
		{
			var returnPersonRequest = Session.CreateCriteria(typeof(PersonRequest), "req")
				.Add(Restrictions.Eq("Id", id))
				.Fetch("requests")
				.Fetch("Person")
				.UniqueResult<IPersonRequest>();

			if (returnPersonRequest?.Request is IShiftTradeRequest shiftTrade)
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
					.Fetch("requests")
					.List<IPersonRequest>());
			}

			foreach (var returnPersonRequest in returnPersonRequests)
			{
				if (returnPersonRequest.Request is IAbsenceRequest absenceRequest)
				{
					LazyLoadingManager.Initialize(absenceRequest.Absence);
				}
			}
			return returnPersonRequests;
		}

		public IEnumerable<IPersonRequest> FindAbsenceAndTextRequests(RequestFilter filter)
		{
			return FindAbsenceAndTextRequests(filter, out _, true);
		}

		public IEnumerable<IPersonRequest> FindAbsenceAndTextRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			var criteria = Session.CreateCriteria<IPersonRequest>("personRequests");
			criteria.Fetch("requests");
			criteria.CreateCriteria("Person", "p", JoinType.InnerJoin);
			criteria.CreateCriteria("requests", "req", JoinType.InnerJoin);

			filterRequestByPeriod(criteria, filter);
			filterRequestByPersons(criteria, filter.Persons);
			filterRequestBelongsToUndeleted(criteria);
			filterRequestByRequestType(criteria, filter.RequestTypes);
			filterRequestByRequestFilters(criteria, filter.RequestFilters, addAbsenceAndTextTypeCriterion);
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

		public IEnumerable<IPersonRequest> FindShiftTradeRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			var criteria = Session.CreateCriteria<IPersonRequest>("personRequests");
			criteria.Fetch("requests");
			criteria.CreateCriteria("Person", "p", JoinType.InnerJoin);
			criteria.CreateCriteria("requests", "req", JoinType.InnerJoin);

			filterRequestByPeriod(criteria, filter);
			filterRequestByShiftTradePersons(criteria, filter.Persons);
			filterRequestBelongsToUndeleted(criteria);
			filterRequestByRequestType(criteria, filter.RequestTypes);
			filterRequestByRequestFilters(criteria, filter.RequestFilters, null);
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
		public IEnumerable<IPersonRequest> FindShiftTradeRequestsByOfferId(Guid offerId)
		{
			var criteria = Session.CreateCriteria<IPersonRequest>("personRequests");
			criteria.Fetch("requests");
			criteria.CreateCriteria("Person", "p", JoinType.InnerJoin);
			criteria.CreateCriteria("requests", "req", JoinType.InnerJoin);
			criteria.Add(toRequestClassTypeConstraint(RequestType.ShiftTradeRequest));
			criteria.Add(Restrictions.Eq("req.Offer.Id", offerId));
			return criteria.List<IPersonRequest>();
		}

		public IEnumerable<IPersonRequest> FindOvertimeRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			var criteria = Session.CreateCriteria<IPersonRequest>("personRequests");
			criteria.Fetch("requests");
			criteria.CreateCriteria("Person", "p", JoinType.InnerJoin);
			criteria.CreateCriteria("requests", "req", JoinType.InnerJoin);

			filterRequestByPeriod(criteria, filter);
			filterRequestByPersons(criteria, filter.Persons);
			filterRequestBelongsToUndeleted(criteria);
			filterRequestByRequestType(criteria, filter.RequestTypes);
			filterRequestByRequestFilters(criteria, filter.RequestFilters, addOvertimeTypeCriterion);
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
			var period = new DateTimePeriod(filter.Period.StartDateTime.Truncate(TimeSpan.FromMinutes(1)),
				filter.Period.EndDateTime.Truncate(TimeSpan.FromMinutes(1)));

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

		private void filterRequestByShiftTradePersons(ICriteria criteria, IEnumerable<IPerson> persons)
		{
			if (persons == null) return;

			var people = persons.ToArray();

			var personIn = createPersonInCriterion("Person", people);
			var personInShiftTradeTo = includeRequestsWithShiftTradePersonTo(people);

			criteria.Add(Restrictions.Or(personInShiftTradeTo, personIn));
		}

		private void filterRequestByPersons(ICriteria criteria, IEnumerable<IPerson> persons)
		{
			if (persons == null) return;

			var people = persons.ToArray();

			var personIn = createPersonInCriterion("Person", people);

			criteria.Add(personIn);
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
			IDictionary<RequestFilterField, string> requestFilters, Action<ICriteria, KeyValuePair<RequestFilterField, string>> typeFilter)
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
						typeFilter(criteria, filter);
						break;

					default:
						continue;
				}
			}
		}

		private void addAbsenceAndTextTypeCriterion(ICriteria criteria, KeyValuePair<RequestFilterField, string> filter)
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

		private void addOvertimeTypeCriterion(ICriteria criteria, KeyValuePair<RequestFilterField, string> filter)
		{
			var ids =
				filter.Value.Split(new[] {splitter}, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => Guid.TryParse(x, out Guid result) ? result : Guid.Empty)
					.Where(x => x != Guid.Empty);
			var overtimeTypeCriterion = ids
				.Select(x => (ICriterion) Restrictions.Eq("req.MultiplicatorDefinitionSet.Id", x))
				.Aggregate(Restrictions.Or);
			criteria.Add(overtimeTypeCriterion);
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
				.Select(x => (ICriterion)Restrictions.Eq("personRequests.requestStatus", x))
				.Aggregate(Restrictions.Or);
			criteria.Add(statusCriterion);
		}

		private static void addAutoDeniedFilterIfDeniedFilterIsIncluded(ICollection<int> statusFilters)
		{
			if (statusFilters.Contains((int)PersonRequestStatus.Denied))
			{
				statusFilters.Add((int)PersonRequestStatus.AutoDenied);
			}
		}

		private static ICriterion getStringFilterCriteria(string filterKeywords, string propertyPath)
		{
			var subjectKeywords = filterKeywords.Split(splitter)
				.Select(x => x.Trim().ToLower())
				.Where(x => !string.IsNullOrEmpty(x)).ToList();

			if (!subjectKeywords.Any()) return null;

			var criteria = subjectKeywords
				.Select(x => (ICriterion)Restrictions.Like(propertyPath, $"%{x}%"))
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

				case RequestType.OvertimeRequest:
					type = typeof(OvertimeRequest);
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
				filteredRequests = filteredRequests.Where(x => x.Request.Period.StartDateTime >= earliestDate);
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

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person)
		{
			return findAllRequestsForAgent(person);
		}

		public IEnumerable<DateTimePeriod> GetRequestPeriodsForAgent(IPerson person, DateTimePeriod period)
		{
			var query = Session.CreateSQLQuery(@"
									SELECT StartDateTime,EndDateTime
									FROM PersonRequest AS pr with(nolock)
									JOIN Request AS req with(nolock) on(req.Parent = pr.id)
									WHERE req.StartDateTime <= :EndDateTime
										AND req.EndDateTime >= :StartDateTime
										AND pr.IsDeleted = :IsDeleted
										AND pr.Person = :Person
										AND pr.BusinessUnit = :BusinessUnit
										AND NOT EXISTS
										(SELECT 1 FROM ShiftTradeRequest sr with(nolock)
										WHERE sr.Request = req.Id)");

			return query.SetDateTime("StartDateTime", period.StartDateTime)
				.SetDateTime("EndDateTime", period.EndDateTime)
				.SetBoolean("IsDeleted", false)
				.SetGuid("BusinessUnit", ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.SetGuid("Person", person.Id.GetValueOrDefault())
				.SetResultTransformer(Transformers.AliasToBean(typeof(RequestPeriod)))
				.SetReadOnly(true)
				.List<RequestPeriod>()
				.Select(p => new DateTimePeriod(TimeZoneHelper.ConvertToUtc(p.StartDateTime, TimeZoneInfo.Utc), TimeZoneHelper.ConvertToUtc(p.EndDateTime, TimeZoneInfo.Utc)));
		}

		private IEnumerable<IPersonRequest> findAllRequestsForAgent(IPerson person)
		{
			var requestsCreatedByPerson = getRequestsCreatedByAgent(person).List<IPersonRequest>();
			var shiftTradeRequestsWithPerson = getShiftTradeRequestsWithAgent(person).List<IPersonRequest>();

			return requestsCreatedByPerson.Union(shiftTradeRequestsWithPerson).OrderByDescending(x => x.UpdatedOn);
		}

		/// <summary>
		/// Get all requests created by agent
		/// </summary>
		private ICriteria getRequestsCreatedByAgent(IPerson person)
		{
			var criteriaPersonRequestsCreatedByPerson = Session.CreateCriteria<PersonRequest>()
				.Fetch("requests")
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.Add(Restrictions.Eq("Person", person));

			return criteriaPersonRequestsCreatedByPerson;
		}

		/// <summary>
		/// Get shift trade request created by other agent but trade with current person
		/// </summary>
		private ICriteria getShiftTradeRequestsWithAgent(IPerson person)
		{
			var subQueryShiftTradeRequestsWithPerson = DetachedCriteria.For<ShiftTradeRequest>()
				.SetProjection(Projections.Property("Parent"))
				.CreateCriteria("ShiftTradeSwapDetails", "swapDetail", JoinType.InnerJoin)
				.Add(Restrictions.Eq("swapDetail.PersonTo", person));

			var criteriaShiftTradeRequestsWithPerson = Session.CreateCriteria<PersonRequest>()
				.Fetch("requests")
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.Add(Restrictions.Not(Restrictions.Eq("requestStatus", 4)))
				.Add(Subqueries.PropertyIn("requests", subQueryShiftTradeRequestsWithPerson));

			return criteriaShiftTradeRequestsWithPerson;
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
				.Fetch("requests")
				.List<IPersonRequest>();

			return retList;
		}

		private IEnumerable<IPersonRequest> findModifiedWithinPeriodOrPending(IPerson person, DateTimePeriod period)
		{
			var retList = Session.CreateCriteria(typeof(PersonRequest))
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Or(Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
					Restrictions.Eq("requestStatus", 0)))
				.Fetch("requests")
				.List<IPersonRequest>();
			return retList;
		}

		public IList<IPersonRequest> FindPersonRequestWithinPeriod(DateTimePeriod period)
		{
			var allRequestExceptShiftTrade = findPersonRequestWithinPeriodExceptShiftTrade(period);
			var allShiftTradeRequests = findShiftTradeRequestWithinPeriod(period);

			return allRequestExceptShiftTrade.Union(allShiftTradeRequests).ToList();
		}

		private IEnumerable<IPersonRequest> findPersonRequestWithinPeriodExceptShiftTrade(DateTimePeriod period)
		{
			var requestForPeriod = DetachedCriteria.For<Request>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Between("Period.period.Minimum", period.StartDateTime, period.EndDateTime))
				.Add(Restrictions.Not(Restrictions.Eq("class", typeof(ShiftTradeRequest))));

			return Session.CreateCriteria<IPersonRequest>()
				.Add(Restrictions.Not(Restrictions.Eq("requestStatus", 3)))
				.Fetch("requests")
				.Fetch("Person")
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
				.Fetch("requests")
				.Fetch("Person")
				.Fetch("requests.ShiftTradeSwapDetails")
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
						.Fetch("requests")
						.Fetch("requests.ShiftTradeSwapDetails")
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
					.CreateAlias("requests","req")
					.Add(Restrictions.Not(Restrictions.Eq("req.class",typeof(ShiftTradeRequest))))
					.Fetch("requests")
					.Fetch("requests.ShiftTradeSwapDetails")
					.Add(Restrictions.InG("Person", item.ToArray()))
					.Add(Restrictions.Or(Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
						Restrictions.Eq("requestStatus", 0)))
					.List<IPersonRequest>());
			}
			
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

		public IEnumerable<IShiftExchangeOffer> FindShiftExchangeOffersForBulletin(DateOnly shiftTradeDate)
		{
			return Session.CreateCriteria(typeof(IShiftExchangeOffer), "offer")
						.CreateCriteria("Parent", "req", JoinType.InnerJoin)
						.Add(Restrictions.Eq("req.IsDeleted", false))
						.Add(Restrictions.Eq("offer.Date", shiftTradeDate))
						.Add(Restrictions.Ge("offer.Criteria.ValidTo", DateOnly.Today))
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
			return Session.CreateSQLQuery(
				  @"SELECT DISTINCT pr.Id from PersonRequest pr, request r WITH(NOLOCK) 
					where
				  		pr.id = r.parent and 
						pr.RequestStatus = 5 and
				  		r.startDateTime <= :endDate and 
						r.endDateTime >= :startDate and 
						pr.IsDeleted = 0")
										  .SetDateTime("startDate", dateTimePeriod.StartDateTime)
										  .SetDateTime("endDate", dateTimePeriod.EndDateTime)
										  .List<Guid>();
		}

		public bool HasWaitlistedRequestsOnSkill(IEnumerable<Guid> skillIds, DateTime startDateTime, DateTime endDateTime, DateTime expiredDateTime)
		{
			var query = Session.CreateSQLQuery(
					@"SELECT count(*) FROM PersonRequest pr
INNER JOIN Request r ON r.Parent = pr.Id
INNER JOIN Person p ON p.Id = pr.Person
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN PersonSkill ps ON pp.Id = ps.Parent
WHERE RequestStatus = 5
AND pr.IsDeleted = 0
AND ((r.StartDateTime BETWEEN :startDate AND :endDate) OR (r.EndDateTime BETWEEN  :startDate AND :endDate))
AND r.StartDateTime > :expired
AND ((pp.StartDate BETWEEN :startDate AND :endDate) OR (pp.EndDate > :startDate AND pp.StartDate < :endDate))
AND ps.Skill in(:list)")
				.SetDateTime("startDate", startDateTime)
				.SetDateTime("endDate", endDateTime)
				.SetDateTime("expired", expiredDateTime)
				.SetParameterList("list", skillIds);

			var cnt = query.UniqueResult<int>();

			return cnt> 0;
		}

		public IList<PersonWaitlistedAbsenceRequest> GetPendingAndWaitlistedAbsenceRequests(DateTimePeriod period,
			Guid? budgetGroupId,
			WaitlistProcessOrder waitlistProcessOrder = WaitlistProcessOrder.FirstComeFirstServed)
		{
			var sqlTemplate = @"SELECT pr.Id as PersonRequestId, pr.RequestStatus
						FROM dbo.PersonRequest pr 
						INNER JOIN dbo.Request r ON r.Parent = pr.Id 
						INNER JOIN dbo.AbsenceRequest ar ON r.Id=ar.Request 
						INNER JOIN dbo.Person p ON p.id=pr.Person
						INNER JOIN dbo.PersonPeriod pp ON pp.Parent = pr.Person 
						INNER JOIN dbo.WorkflowControlSet wcs ON wcs.Id = p.WorkflowControlSet
						{0}
						WHERE pr.BusinessUnit = :businessUnit
						AND p.IsDeleted = 0
						AND wcs.IsDeleted = 0
						AND  r.EndDateTime > :startDate and r.StartDateTime < :endDate
						AND :withInPersonPeriod BETWEEN pp.StartDate AND pp.EndDate
						AND pr.RequestStatus in (0,5)
						{1}
						{2}";

			string joinSql, budgetGroupSql, orderSql;
			if (waitlistProcessOrder == WaitlistProcessOrder.BySeniority)
			{
				joinSql = @"INNER JOIN (SELECT Parent, FirstStart = min(pp_inner.StartDate) 
						          FROM dbo.PersonPeriod pp_inner
						          GROUP BY Parent) as pps
						     ON (pr.Person = pps.Parent)";
				orderSql = "ORDER BY pps.FirstStart, pr.CreatedOn";
			}
			else
			{
				joinSql = "";
				orderSql = "ORDER BY pr.CreatedOn";
			}

			if (budgetGroupId.HasValue)
			{
				budgetGroupSql = "AND pp.BudgetGroup= :budgetGroupId";
			}
			else
			{
				budgetGroupSql = "AND pp.BudgetGroup is null";
			}

			var fullSql = string.Format(sqlTemplate, joinSql, budgetGroupSql, orderSql);

			var iSqlQuery = Session.CreateSQLQuery(fullSql)
				.SetDateTime("startDate", period.StartDateTime)
				.SetDateTime("endDate", period.EndDateTime)
				.SetDateTime("withInPersonPeriod", period.StartDateTime.Date)
				.SetGuid("businessUnit", ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.GetValueOrDefault());

			if (budgetGroupId.HasValue)
			{
				iSqlQuery.SetGuid(nameof(budgetGroupId), budgetGroupId.Value);
			}

			return iSqlQuery.SetResultTransformer(Transformers.AliasToBean<PersonWaitlistedAbsenceRequest>())
				.List<PersonWaitlistedAbsenceRequest>();
		}

		public IList<IPersonRequest> FindPersonRequestsWithAbsenceAndPersonPeriods(IEnumerable<Guid> ids)
		{
			var returnPersonRequests = new List<IPersonRequest>();
			foreach (var idBatch in ids.Batch(1000))
			{
				returnPersonRequests.AddRange(Session.CreateCriteria(typeof(PersonRequest), "req")
					.Add(Restrictions.In("Id", idBatch.ToArray()))
					.Fetch("requests")
					.List<IPersonRequest>());
			}

			foreach (var returnPersonRequest in returnPersonRequests)
			{
				if (returnPersonRequest.Request is IAbsenceRequest absenceRequest)
				{
					LazyLoadingManager.Initialize(absenceRequest.Absence);
					LazyLoadingManager.Initialize(absenceRequest.Person.PersonPeriodCollection);
				}
			}
			return returnPersonRequests;
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