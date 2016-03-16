using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
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
			throw new NotImplementedException();
		}

		public IPersonRequest Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPersonRequest> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IList<IPersonRequest> Find(IPerson person, DateTimePeriod period)
		{
			throw new NotImplementedException();
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
			return new List<IPersonRequest>();
		}

		public IEnumerable<IPersonRequest> FindAllRequests(RequestFilter filter)
		{
			int count;
			return FindAllRequests(filter, out count, true);
		}


		public IEnumerable<IPersonRequest> FindAllRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			List<IPersonRequest> requests;

			if (filter.ExcludeRequestsOnFilterPeriodEdge)
			{
				requests = (from request in _requestRepository
							where request.Request.Period.EndDateTime > filter.Period.StartDateTime &&
									request.Request.Period.StartDateTime < filter.Period.EndDateTime
							select request).ToList();
			}
			else
			{
				requests = _requestRepository.Where(request => filter.Period.ContainsPart(request.Request.Period)).ToList();
			}

			if (filter.RequestTypes != null)
			{
				requests = requests.Where(request => filter.RequestTypes.Contains(request.Request.RequestType)).ToList();
			}

			if (filter.Persons != null)
			{
				requests = requests.Where(request => filter.Persons.Contains(request.Person)).ToList();
			}


			count = requests.Count();
			return requests;
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgentByType(IPerson person, Paging paging,
			params RequestType[] requestTypes)
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
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindByStatus<T>(IPerson person, DateTime startDate, int status) where T : Request
		{
			throw new NotImplementedException();
		}

		public IPersonRequest FindPersonRequestByRequestId(Guid value)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindAllRequestsExceptOffer(IPerson person, Paging paging)
		{
			return _requestRepository.Where(request => request.Request.RequestType != RequestType.ShiftExchangeOffer
														&& request.Person.Id.GetValueOrDefault() == person.Id.GetValueOrDefault())
				.ToList();
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
	}
}