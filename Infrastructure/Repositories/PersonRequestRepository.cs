using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Collection;
using NHibernate;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for person requests
	/// </summary>
	public class PersonRequestRepository : Repository<IPersonRequest>, IPersonRequestRepository
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PersonRequestRepository"/> class.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-08-31
		/// </remarks>
		public PersonRequestRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}

		public PersonRequestRepository(IUnitOfWorkFactory unitOfWorkFactory)
			: base(unitOfWorkFactory)
		{
		}

		/// <summary>
		/// Finds the specified person's request for given period.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Sumedah
		/// Created date: 2008-11-19
		/// </remarks>
		public IList<IPersonRequest> Find(IPerson person, DateTimePeriod period)
		{
			var requestForPeriod = DetachedCriteria.For<Request>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Ge("Period.period.Maximum", period.StartDateTime))
				.Add(Restrictions.Le("Period.period.Minimum", period.EndDateTime))
				;

			return Session.CreateCriteria<IPersonRequest>()
					.Add(Restrictions.Eq("Person", person))
					.SetFetchMode("requests", FetchMode.Join)
					.Add(Subqueries.Exists(requestForPeriod))
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

		public IList<IPersonRequest> FindAllExceptUnapprovedShiftTrades(ICollection<IPerson> persons)
		{
			if (persons.Count == 0) return new List<IPersonRequest>();

			var retList = Session.GetNamedQuery("findAllExceptUnapprovedShiftTrades")
				.SetEnum("ShiftTradeStatus", ShiftTradeStatus.OkByBothParts)
				.SetParameterList("people", persons.ToArray())
				.List<IPersonRequest>();
			fetchAbsences(retList);
			return retList;
		}

		private static void fetchAbsences(IEnumerable<IPersonRequest> personRequests)
		{
			foreach (var pRequest in personRequests)
			{
				var absReq = pRequest.Request as IAbsenceRequest;
				if (absReq != null && !LazyLoadingManager.IsInitialized(absReq.Absence))
					LazyLoadingManager.Initialize(absReq.Absence);
			}
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person) { return FindAllRequestsForAgent(person, null, null); }
		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, Paging paging) { return FindAllRequestsForAgent(person, null, paging); }
		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, DateTimePeriod period) { return FindAllRequestsForAgent(person, period, null); }

		private IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, DateTimePeriod? period, Paging paging)
		{
			var shiftTradeDetailsForAgent = DetachedCriteria.For<ShiftTradeSwapDetail>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Restrictions.Or(
					Restrictions.Eq("PersonFrom", person),
					Restrictions.Eq("PersonTo", person))
				);

			var shiftTradeRequestsForAgent = DetachedCriteria.For<ShiftTradeRequest>()
				.SetProjection(Projections.Property("Parent"))
				.Add(Subqueries.PropertyIn("ShiftTradeSwapDetails", shiftTradeDetailsForAgent));

			var personRequests = Session.CreateCriteria<PersonRequest>()
				.SetFetchMode("requests", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.AddOrder(Order.Desc("UpdatedOn"));

			var requestsForAgent = Restrictions.Or(
				Subqueries.PropertyIn("requests", shiftTradeRequestsForAgent),
				Restrictions.Eq("Person", person));

			personRequests.Add(requestsForAgent);

			if (period != null)
			{
				var requestsForPeriod = DetachedCriteria.For<Request>()
					.SetProjection(Projections.Property("Parent"))
					.Add(Restrictions.Ge("Period.period.Maximum", period.Value.StartDateTime))
					.Add(Restrictions.Le("Period.period.Minimum", period.Value.EndDateTime))
					;
				personRequests.Add(Subqueries.PropertyIn("requests", requestsForPeriod));
			}

			if (paging != null)
			{
				personRequests.SetMaxResults(paging.Take);
				personRequests.SetFirstResult(paging.Skip);
			}

			return personRequests.List<IPersonRequest>();
		}

		public IEnumerable<IPersonRequest> FindTextAndAbsenceRequestsForAgent(IPerson person, Paging paging)
		{
			var personRequests = Session.GetNamedQuery("findTextAndAbsenceRequestsForAgent")
				.SetEntity("person", person);
			if (paging != null)
			{
				personRequests.SetMaxResults(paging.Take);
				personRequests.SetFirstResult(paging.Skip);
			}
			return personRequests.List<IPersonRequest>();
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
			//Alla request d�r UpdatedOn innanf�r period
			IEnumerable<IPersonRequest> allRequestsBy = FindModifiedWithinPeriodOrPending(person, period);

			//Alla shiftTrades d�r UpdatedOn innanf�r Period
			IEnumerable<IPersonRequest> allShiftTradesTo = FindShiftTradesModifiedWithinPeriod(person, period);

			return allRequestsBy.Union(allShiftTradesTo).ToList();
		}

		private IEnumerable<IPersonRequest> FindShiftTradesModifiedWithinPeriod(IPerson person, DateTimePeriod period)
		{
			IList<IPersonRequest> retList =
				Session.CreateCriteria(typeof(PersonRequest))
					.Add
					(
						Restrictions.Or(
							Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
							Restrictions.Eq("requestStatus", 0))
					)
					.Add(Subqueries.PropertyIn("requests", DetachedCriteria.For(typeof(ShiftTradeRequest))
																			.SetProjection(Projections.Property("Parent"))
																			.Add(Subqueries.PropertyIn("ShiftTradeSwapDetails",
																												DetachedCriteria.For<ShiftTradeSwapDetail>()
																													.SetProjection(Projections.Property("Parent"))

																													.Add(
																														Restrictions.Or(
																															Restrictions.Eq("PersonFrom", person),
																															Restrictions.Eq("PersonTo", person)))))))
					.SetFetchMode("requests", FetchMode.Join)
					.List<IPersonRequest>();

			return retList;

		}

		private IEnumerable<IPersonRequest> FindModifiedWithinPeriodOrPending(IPerson person, DateTimePeriod period)
		{
			IList<IPersonRequest> retList = Session.CreateCriteria(typeof(PersonRequest))
				.Add(Restrictions.Eq("Person", person))
				.Add(Restrictions.Or(Restrictions.Between("UpdatedOn", period.StartDateTime, period.EndDateTime),
											Restrictions.Eq("requestStatus", 0)))
				.SetFetchMode("requests", FetchMode.Join)
				.List<IPersonRequest>();
			return retList;
		}

		public IList<IPersonRequest> FindPersonRequestWithinPeriod(DateTimePeriod period)
		{
			IList<IPersonRequest> allRequestExceptShiftTrade = FindPersonRequestWithinPeriodExceptShiftTrade(period);
			IList<IPersonRequest> allShiftTradeRequests = FindShiftTradeRequestWithinPeriod(period);

			return allRequestExceptShiftTrade.Union(allShiftTradeRequests).ToList();
		}

		private IList<IPersonRequest> FindPersonRequestWithinPeriodExceptShiftTrade(DateTimePeriod period)
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

		private IList<IPersonRequest> FindShiftTradeRequestWithinPeriod(DateTimePeriod period)
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
		public IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(ICollection<IPerson> persons, DateTimePeriod period)
		{
			//Alla request d�r UpdatedOn innanf�r period
			IEnumerable<IPersonRequest> allRequestsBy = FindModifiedWithinPeriodOrPending(persons, period);

			//Alla shiftTrades d�r UpdatedOn innanf�r Period
			IEnumerable<IPersonRequest> allShiftTradesTo = FindShiftTradesModifiedWithinPeriod(persons, period);

			return allRequestsBy.Union(allShiftTradesTo).ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private IEnumerable<IPersonRequest> FindShiftTradesModifiedWithinPeriod(IEnumerable<IPerson> persons, DateTimePeriod period)
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
					 .Add(Subqueries.PropertyIn("ShiftTradeSwapDetails", DetachedCriteria.For<ShiftTradeSwapDetail>().SetProjection(Projections.Property("Parent"))
						 .Add(Restrictions.Or(Restrictions.InG("PersonFrom", personChunkList), Restrictions.InG("PersonTo", personChunkList)))))))
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

		private IEnumerable<IPersonRequest> FindModifiedWithinPeriodOrPending(IEnumerable<IPerson> persons, DateTimePeriod period)
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
	}
}
