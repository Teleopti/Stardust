﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRequestRepository : IPersonRequestRepository
	{
		private readonly IList<IPersonRequest> _requestRepository = new List<IPersonRequest>();

		public void Add(IPersonRequest entity)
		{
			_requestRepository.Add(entity);
			if (!entity.Id.HasValue)
			{
				entity.SetId(Guid.NewGuid());
			}
		}

		public void Remove(IPersonRequest entity)
		{
			throw new NotImplementedException();
		}

		public IPersonRequest Get(Guid id)
		{
			return _requestRepository.FirstOrDefault(x => x.Id == id);
		}

		public IList<IPersonRequest> LoadAll()
		{
			return _requestRepository;
		}

		public IPersonRequest Load(Guid id)
		{
			return Get(id);
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IList<IPersonRequest> Find(IPerson person, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> Find(List<Guid> ids)
		{
			return _requestRepository.Where(p => ids.Any(x => x == p.Id)).ToList();
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, Paging paging)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, DateTimePeriod period)
		{
			return _requestRepository;
		}

		public IEnumerable<IPersonRequest> FindAllRequests(RequestFilter filter)
		{
			int count;
			return FindAllRequests(filter, out count, true);
		}

		public IEnumerable<IPersonRequest> FindAllRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			IEnumerable<IPersonRequest> requests;

			if (filter.ExcludeRequestsOnFilterPeriodEdge)
			{
				requests = (from request in _requestRepository
							where request.Request.Period.EndDateTime > filter.Period.StartDateTime &&
								  request.Request.Period.StartDateTime < filter.Period.EndDateTime
							select request);
			}
			else
			{
				requests = _requestRepository.Where(request => filter.Period.ContainsPart(request.Request.Period));
			}

			if (filter.OnlyIncludeRequestsStartingWithinPeriod)
			{
				requests = requests.Where(request => request.Request.Period.StartDateTime >= filter.Period.StartDateTime);
			}

			if (filter.RequestTypes != null)
			{
				requests = requests.Where(request => filter.RequestTypes.Contains(request.Request.RequestType));
			}

			if (filter.Persons != null)
			{
				requests = requests.Where(request => filter.Persons.Contains(request.Person));
			}

			if (filter.ExcludeInvalidShiftTradeRequest)
			{
				requests = requests.Where(request =>
				{
					if (!(request.Request is ShiftTradeRequest))
					{
						return true;
					}
					var shiftTradeStatus = ((ShiftTradeRequest)request.Request)
						.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing());
					return shiftTradeStatus != ShiftTradeStatus.OkByMe && shiftTradeStatus != ShiftTradeStatus.Referred;
				});
			}

			requests = requests.Where(r => !((Person)r.Person).IsDeleted);
			count = requests.Count();
			return requests.ToList();
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgentByType(IPerson person, Paging paging,
			DateTime? earliestDate, params RequestType[] requestTypes)
		{
			return _requestRepository.Where(request => requestTypes.Contains(request.Request.RequestType));
		}

		public IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(IPerson person, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindPersonRequestUpdatedAfter(DateTime lastTime)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(ICollection<IPerson> persons,
			DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IPersonRequest Find(Guid id)
		{
			return _requestRepository.FirstOrDefault(x => x.Id == id);
		}

		public IList<IPersonRequest> FindByStatus<T>(IPerson person, DateTime startDate, int status) where T : Request
		{
			throw new NotImplementedException();
		}

		public IPersonRequest FindPersonRequestByRequestId(Guid value)
		{
			return _requestRepository.FirstOrDefault(x => x.Request.Id.GetValueOrDefault() == value);
		}

		public IList<IShiftExchangeOffer> FindOfferByStatus(IPerson person, DateOnly date, ShiftExchangeOfferStatus status)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IShiftExchangeOffer> FindShiftExchangeOffersForBulletin(IEnumerable<IPerson> personList,
			DateOnly shiftTradeDate)
		{
			var result = _requestRepository.Where(request => request.Request.RequestType == RequestType.ShiftExchangeOffer
															 && request.RequestedDate.Date == shiftTradeDate.Date
															 && personList.Contains(request.Person) && request.IsPending)
				.Select(pr => (IShiftExchangeOffer)pr.Request)
				.ToList();
			return result;
		}

		public IList<Guid> GetWaitlistRequests(DateTimePeriod dateTimePeriod)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindAllRequestsExceptOffer(IPerson person, Paging paging)
		{
			return _requestRepository.Where(request => request.Request.RequestType != RequestType.ShiftExchangeOffer
													   && request.Person.Id.GetValueOrDefault() == person.Id.GetValueOrDefault())
				.ToList();
		}
	}
}