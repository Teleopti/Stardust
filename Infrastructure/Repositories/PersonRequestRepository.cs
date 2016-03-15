using System;
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
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for person requests
	/// </summary>
	public class PersonRequestRepository : Repository<IPersonRequest>, IPersonRequestRepository
	{
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

		public IList<IPersonRequest> Find<T>(IPerson person, DateTimePeriod period) where T : Request
		{
			var requestForPeriod = createRequestForPeriodCriteria(period).Add(Restrictions.Eq("class", typeof(T)));
			return findRequestsByRequestPeriod(person, requestForPeriod);
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
			if (returnPersonRequest != null)
			{
				var shiftTrade = returnPersonRequest.Request as IShiftTradeRequest;
				if (shiftTrade != null)
				{
					LazyLoadingManager.Initialize(shiftTrade.ShiftTradeSwapDetails);
				}
			}

			return returnPersonRequest;
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


			filterRequestByPeriod(criteria, filter.Period);
			filterRequestByPersons(criteria, filter.Persons);
			filterRequestByRequestType(criteria, filter.RequestTypes);

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

		private void filterRequestByPeriod(ICriteria criteria, DateTimePeriod period)
		{
			var requestForPeriod = DetachedCriteria.For<Request>()
				.SetProjection (Projections.Property ("Parent"))
				.Add (Restrictions.Ge ("Period.period.Maximum", period.StartDateTime))
				.Add (Restrictions.Le ("Period.period.Minimum", period.EndDateTime));

			criteria.Add(Subqueries.PropertyIn("Id", requestForPeriod));
		}

		private void filterRequestByPersons(ICriteria criteria, IEnumerable<IPerson> persons)
		{
			if (persons == null) return;
			criteria.Add(Restrictions.In("Person", persons.ToArray()));
		}

		private void filterRequestByRequestType(ICriteria criteria, IEnumerable<RequestType> requestTypes)
		{
			if (requestTypes == null) return;
			var typeCriterion = requestTypes.Select(toRequestClassTypeConstraint)
					.Where(c => c != null).Aggregate(Restrictions.Or);
			criteria.Add(typeCriterion);
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
				default:
					return null;
			}
			return Restrictions.Eq("req.class", type);
		}
		
		private void pagePersonRequests(ICriteria query, IPaging paging)
		{
			if (paging == null || (paging as Paging).Equals(Paging.Nothing)) return;		
			query.SetFirstResult(paging.Skip).SetMaxResults(paging.Take);
		}
		
		public IEnumerable<IPersonRequest> FindAllRequestsForAgentByType(IPerson person, Paging paging, params RequestType[] requestTypes)
		{
			var allRequests = FindAllRequestsForAgent(person);
			var filteredRequests =
				allRequests.Where(x => requestTypes.Contains(x.Request.RequestType)).OrderByDescending(x => x.UpdatedOn);

			return paging == null ? filteredRequests : filteredRequests.Skip(paging.Skip).Take(paging.Take);
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
			if (paging == null) return;

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

			var retList =
				Session.CreateCriteria(typeof(PersonRequest))
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

		public IList<IPersonRequest> FindPersonRequestUpdatedAfter(DateTime lastTime)
		{
			return Session.CreateCriteria<PersonRequest>()
				.Add(Restrictions.Not(Restrictions.Eq("requestStatus", 3)))
				.Add(Restrictions.Ge("UpdatedOnServerUtc", lastTime))
				.SetFetchMode("requests", FetchMode.Join)
				.SetFetchMode("Person", FetchMode.Join)
				.SetFetchMode("requests.ShiftTradeSwapDetails", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IPersonRequest>();
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
									.Add(Restrictions.Or(Restrictions.InG("PersonFrom", personChunkList),
										Restrictions.InG("PersonTo", personChunkList)))))))
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
			if (entity.IsDenied)
				throw new DataSourceException("Cannot delete person request " + entity + ". It has already been denied.");
			if (entity.IsApproved)
				throw new DataSourceException("Cannot delete person request " + entity + ". It has already been approved.");
			base.Remove(entity);
		}

		public IEnumerable<IShiftExchangeOffer> FindShiftExchangeOffersForBulletin(IEnumerable<IPerson> personList,
			DateOnly shiftTradeDate)
		{
			return Session.CreateCriteria(typeof(IShiftExchangeOffer))
				.Add(Restrictions.Eq("Date", shiftTradeDate))
				.Add(Restrictions.In("Person", personList.ToList()))
				.Add(Restrictions.Ge("Criteria.ValidTo", new DateOnly(DateTime.UtcNow.Date)))
				.Add(Restrictions.Eq("Status", ShiftExchangeOfferStatus.Pending))
				.List<ShiftExchangeOffer>();
		}

		private void sortPersonRequests(ICriteria criteria, IList<RequestsSortingOrder> sortingOrders)
		{
			if (sortingOrders == null) return;
			var orderMapping = new Dictionary<RequestsSortingOrder, Action<ICriteria>>
				{
					{RequestsSortingOrder.AgentNameAsc, c => c.AddOrder(Order.Asc("p.Name.LastName"))},
					{RequestsSortingOrder.AgentNameDesc, c => c.AddOrder(Order.Desc("p.Name.LastName"))},				
					{RequestsSortingOrder.SubjectAsc, c => c.AddOrder(Order.Asc("personRequests.Subject"))},
					{RequestsSortingOrder.SubjectDesc, c => c.AddOrder(Order.Desc("personRequests.Subject"))},				
					{RequestsSortingOrder.CreatedOnAsc, c => c.AddOrder(Order.Asc("personRequests.CreatedOn"))},
					{RequestsSortingOrder.CreatedOnDesc, c => c.AddOrder(Order.Desc("personRequests.CreatedOn"))},
					{RequestsSortingOrder.UpdatedOnAsc, c => c.AddOrder(Order.Asc("personRequests.UpdatedOn"))},
					{RequestsSortingOrder.UpdatedOnDesc, c => c.AddOrder(Order.Desc("personRequests.UpdatedOn"))},				
					{RequestsSortingOrder.PeriodStartAsc, c => c.AddOrder(Order.Asc("req.Period.period.Minimum"))},
					{RequestsSortingOrder.PeriodStartDesc, c => c.AddOrder(Order.Desc("req.Period.period.Minimum"))},
					{RequestsSortingOrder.PeriodEndAsc, c => c.AddOrder(Order.Asc("req.Period.period.Maximum"))},
					{RequestsSortingOrder.PeriodEndDesc, c => c.AddOrder(Order.Desc("req.Period.period.Maximum"))}					
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
	}
}
