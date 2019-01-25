using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	public class AbsenceRequestWaitlistProvider80131On : IAbsenceRequestWaitlistProvider
	{
		private readonly IPersonRequestRepository _personRequestRepository;

		public AbsenceRequestWaitlistProvider80131On(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}

		public IList<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period)
		{
			var waitListIds = _personRequestRepository.GetWaitlistRequests(period).ToList();
			return _personRequestRepository.Find(waitListIds).ToList();
		}

		public int GetPositionInWaitlist(IAbsenceRequest absenceRequest)
		{
			if (!(absenceRequest.Parent is PersonRequest personRequest) || !personRequest.IsWaitlisted) return 0;

			var requestsInWaitlist = getRequestsInWaitlist(absenceRequest).ToList();
			var index = requestsInWaitlist.FindIndex(perRequest => perRequest == personRequest.Id);
			return index > -1 ? index + 1 : 0;
		}

		private IEnumerable<Guid> getRequestsInWaitlist(IAbsenceRequest absenceRequest)
		{
			var period = absenceRequest.Period.ChangeEndTime(TimeSpan.FromSeconds(-1));

			var budgetGroupId = getAbsenceRequestPersonFirstBudgetGroup(absenceRequest);
			var processOrder = absenceRequest.Person.WorkflowControlSet.AbsenceRequestWaitlistProcessOrder;
			var requests = _personRequestRepository.GetPendingAndWaitlistedAbsenceRequests(period, budgetGroupId, processOrder);
			var list = _personRequestRepository.FindPersonReuqestsWithAbsenceAndPersonPeriods(requests.Select(x => x.PersonRequestId));
			var personRequestDic = list.ToDictionary(personRequest => personRequest.Id.Value);
			var waitlistedRequests = requests.Where(x => requestIsAutoGrant(x, personRequestDic)).Select(x => x.PersonRequestId);
			return waitlistedRequests;
		}

		private bool requestIsAutoGrant(PersonWaitlistedAbsenceRequest personWaitlistedAbsenceRequest,Dictionary<Guid,IPersonRequest> personRequestDic)
		{
			if (personWaitlistedAbsenceRequest.RequestStatus == PersonRequestStatus.Waitlisted)
				return true;

			var request = personRequestDic[personWaitlistedAbsenceRequest.PersonRequestId];
			var wcs = request.Person.WorkflowControlSet;
			if (wcs == null)
				return false;

			var isAutoGrant = wcs.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)request.Request)
								  .AbsenceRequestProcess.GetType() == typeof(GrantAbsenceRequest);
			return isAutoGrant;
		}

		private Guid? getAbsenceRequestPersonFirstBudgetGroup(IAbsenceRequest request)
		{
			var period = request.Period.ChangeEndTime(TimeSpan.FromSeconds(-1));
			var requestPerson = request.Person;
			var personPeriod = requestPerson.PersonPeriods(period.ToDateOnlyPeriod(requestPerson.PermissionInformation.DefaultTimeZone())).FirstOrDefault();

			Guid? budgetGroupId = null;
			if (personPeriod?.BudgetGroup?.Id != null)
			{
				budgetGroupId = personPeriod.BudgetGroup.Id.Value;
			}

			return budgetGroupId;
		}
	}
}