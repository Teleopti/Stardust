using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRequestRepositoryWithClone : FakePersonRequestRepository
	{
		public override void Add(IPersonRequest entity)
		{
			if (!entity.Id.HasValue)
			{
				entity.SetId(Guid.NewGuid());
			}
			var clone = (PersonRequest)entity.Clone();
			clone.SetId(entity.Id);
			RequestRepository.Add(clone);
		}
	}

	public class FakePersonRequestRepository : IPersonRequestRepository
	{
		public readonly IList<IPersonRequest> RequestRepository = new List<IPersonRequest>();
		public bool HasWaitlisted;
		public virtual void Add(IPersonRequest entity)
		{
			if (!entity.Id.HasValue)
			{
				entity.SetId(Guid.NewGuid());
			}
			RequestRepository.Add(entity);
		}

		public void Remove(IPersonRequest entity)
		{
			throw new NotImplementedException();
		}

		public IPersonRequest Get(Guid id)
		{
			return RequestRepository.FirstOrDefault(x => x.Id == id);
		}

		public IList<IPersonRequest> LoadAll()
		{
			return RequestRepository;
		}

		public IPersonRequest Load(Guid id)
		{
			return Get(id);
		}

		public IList<IPersonRequest> Find(IPerson person, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> Find(IEnumerable<Guid> ids)
		{
			return RequestRepository.Where(p => ids.Any(x => x == p.Id)).ToList();
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
			return RequestRepository;
		}

		public IEnumerable<IPersonRequest> FindAbsenceAndTextRequests(RequestFilter filter)
		{
			int count;
			return findAllRequests(filter, out count, true);
		}

		private IEnumerable<IPersonRequest> findAllRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			IEnumerable<IPersonRequest> requests;

			if (filter.ExcludeRequestsOnFilterPeriodEdge)
			{
				requests = (from request in RequestRepository
							where request.Request.Period.EndDateTime > filter.Period.StartDateTime &&
								  request.Request.Period.StartDateTime < filter.Period.EndDateTime
							select request);
			}
			else
			{
				requests = RequestRepository.Where(request => filter.Period.ContainsPart(request.Request.Period));
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

		public IEnumerable<IPersonRequest> FindAbsenceAndTextRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			return findAllRequests(filter, out count, ignoreCount);
		}

		public IEnumerable<IPersonRequest> FindShiftTradeRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			return findAllRequests(filter, out count, ignoreCount);
		}

		public IEnumerable<IPersonRequest> FindOvertimeRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			return findAllRequests(filter, out count, ignoreCount);
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgentByType(IPerson person, Paging paging,
			DateTime? earliestDate, params RequestType[] requestTypes)
		{
			return RequestRepository.Where(request => requestTypes.Contains(request.Request.RequestType)).OrderByDescending(x=>x.UpdatedOn);
		}

		public IEnumerable<IPersonRequest> FindAllRequestsSortByRequestedDate(IPerson person, Paging paging, DateTime? earliestDate,
			params RequestType[] requestTypes)
		{
			return RequestRepository.Where(request => requestTypes.Contains(request.Request.RequestType)).OrderByDescending(x=>x.RequestedDate);
		}

		public IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(IPerson person, DateTimePeriod period)
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
			return RequestRepository.FirstOrDefault(x => x.Id == id);
		}

		public IList<IPersonRequest> FindByStatus<T>(IPerson person, DateTime startDate, int status) where T : Request
		{
			throw new NotImplementedException();
		}

		public IPersonRequest FindPersonRequestByRequestId(Guid value)
		{
			return RequestRepository.FirstOrDefault(x => x.Request.Id.GetValueOrDefault() == value);
		}

		public IList<IShiftExchangeOffer> FindOfferByStatus(IPerson person, DateOnly date, ShiftExchangeOfferStatus status)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IShiftExchangeOffer> FindShiftExchangeOffersForBulletin(DateOnly shiftTradeDate)
		{
			var result = RequestRepository.Where(request => request.Request.RequestType == RequestType.ShiftExchangeOffer
															 && request.RequestedDate.Date == shiftTradeDate.Date)
				.Select(pr => (IShiftExchangeOffer)pr.Request)
				.ToList();
			return result;
		}

		public IList<Guid> GetWaitlistRequests(DateTimePeriod dateTimePeriod)
		{
			return RequestRepository.Where(request => request.IsWaitlisted && request.Request.Period.Intersect(dateTimePeriod)).Select(r => r.Id.GetValueOrDefault()).ToList();
		}

	    public IList<IPersonRequest> FindPersonRequestWithinPeriod(DateTimePeriod period)
	    {
	        throw new NotImplementedException();
	    }

		public IEnumerable<DateTimePeriod> GetRequestPeriodsForAgent(IPerson person, DateTimePeriod period)
		{
			return RequestRepository.Select(p => p.Request.Period);
		}

		public bool HasWaitlistedRequestsOnSkill(IEnumerable<Guid> skills, DateTime startDateTime, DateTime endDateTime, DateTime expiredDateTime)
		{
			return HasWaitlisted;
		}
	}
}