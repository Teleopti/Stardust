using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[InstancePerLifetimeScope]
	public class MultiAbsenceRequestsHandler : IHandleEvent<NewMultiAbsenceRequestsCreatedEvent>, IRunOnStardust
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IStardustJobFeedback _feedback;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IMultiAbsenceRequestsUpdater _multiAbsenceRequestsUpdater;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly IAbsenceRequestWaitlistProvider _absenceRequestWaitlistProvider;
		private readonly IFilterRequests _filterRequests;

		public MultiAbsenceRequestsHandler(IPersonRequestRepository personRequestRepository,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IStardustJobFeedback stardustJobFeedback, IWorkflowControlSetRepository workflowControlSetRepository,
			IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IMultiAbsenceRequestsUpdater multiAbsenceRequestsUpdater, 
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, IFilterRequests filterRequests, IAbsenceRequestWaitlistProvider absenceRequestWaitlistProvider)
		{
			_personRequestRepository = personRequestRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_feedback = stardustJobFeedback;
			_workflowControlSetRepository = workflowControlSetRepository;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_multiAbsenceRequestsUpdater = multiAbsenceRequestsUpdater;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_filterRequests = filterRequests;
			_absenceRequestWaitlistProvider = absenceRequestWaitlistProvider;
		}

		[AsSystem]
		public virtual void Handle(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			_feedback.SendProgress($"Received {@event.PersonRequestIds.Count} Absence Requests.");

			IList<IPersonRequest> personRequests;
			IDictionary<Guid, IEnumerable<IAbsenceRequestValidator>> requestValidators;
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				personRequests = checkPersonRequest(@event);
				requestValidators = getMandatoryValidators(personRequests);
			}

			_feedback.SendProgress($"Done Checking Person Requests. {personRequests.Count} will be processed.");
			var itsLockSafe = true;
			try
			{
				if (!personRequests.IsNullOrEmpty())
				{
					var personRequestIds = personRequests.Select(p => p.Id.GetValueOrDefault()).ToList();
					_multiAbsenceRequestsUpdater.UpdateAbsenceRequest(personRequestIds, requestValidators);
				}
			}
			catch (Exception e)
			{
				if (e.IsSqlDeadlock())
					itsLockSafe = false;
				else
					throw;
			}
			if (itsLockSafe)
			{
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					_queuedAbsenceRequestRepository.Remove(@event.Sent);
					uow.PersistAll();
				}
			}
			else
			{
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					//reset sent column
					_queuedAbsenceRequestRepository.ResetSent(@event.Sent);
					uow.PersistAll();
				}
			}
			
		}

		private List<IPersonRequest> checkPersonRequest(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			DateTime min = DateTime.MaxValue;
			DateTime max = DateTime.MinValue;
			var requests = new List<IPersonRequest>();
			var atleastOneWaitlistedWfc = false;


			var wfcs = _workflowControlSetRepository.LoadAll();
			if (wfcs.Any(x => x.AbsenceRequestWaitlistEnabled))
			{
				atleastOneWaitlistedWfc = true;

				var queuedRequests = _queuedAbsenceRequestRepository.LoadAll().Where(x => x.Sent.HasValue && DateTime.Compare(x.Sent.Value, @event.Sent.Truncate(TimeSpan.FromSeconds(1))) == 0);  
				foreach (var request in queuedRequests)
				{
					if (request.StartDateTime < min)
						min = request.StartDateTime;
					if (request.EndDateTime > max)
						max = request.EndDateTime;
				}
			}

			var personRequests = _personRequestRepository.Find(@event.PersonRequestIds);

			foreach (var personRequest in personRequests)
			{
				if (personRequestSpecification.IsSatisfiedBy(personRequest))
				{
					string warning =
						$"No person request found with the supplied Id or the request is not in pending status mode. (Id = {personRequest.Id})";
					_feedback.SendProgress(warning);
				}
				else if (absenceRequestSpecification.IsSatisfiedBy((IAbsenceRequest) personRequest.Request))
				{
					string warning = $"The found person request is not of type absence request. (Id = {personRequest.Id})";
					_feedback.SendProgress(warning);
				}
				else
				{
					requests.Add(personRequest);
				}

			}
			if (requests.Any())
			{
				if (requests.Min(x => x.Request.Period.StartDateTime) < min)
					min = requests.Min(x => x.Request.Period.StartDateTime);
				if (requests.Max(x => x.Request.Period.EndDateTime) > max)
					max = requests.Max(x => x.Request.Period.EndDateTime);
			}

			if (max > min && atleastOneWaitlistedWfc)
			{
				DateTimePeriod period = new DateTimePeriod(min.Utc(), max.Utc());
				var waitlistedRequests = _absenceRequestWaitlistProvider.GetWaitlistedRequests(period);
				requests.AddRange(_filterRequests.Filter(waitlistedRequests.ToList()));
				_feedback.SendProgress($"Picked up {waitlistedRequests.Count} waitlisted requests in period {period}.");
			}


			return requests;
		}

		private IDictionary<Guid, IEnumerable<IAbsenceRequestValidator>> getMandatoryValidators(IList<IPersonRequest> personRequests)
		{
			var requestValidators = new Dictionary<Guid, IEnumerable<IAbsenceRequestValidator>>();
			var personRequestIds = personRequests.Select(p => p.Id.GetValueOrDefault()).ToList();
			var queuedAbsenceRequests = _queuedAbsenceRequestRepository.FindByPersonRequestIds(personRequestIds);
			foreach (var queuedAbsenceRequest in queuedAbsenceRequests)
			{
				var mandatoryValidators = queuedAbsenceRequest.MandatoryValidators;
				if (requestValidators.ContainsKey(queuedAbsenceRequest.PersonRequest))
					continue;
				var personRequest = personRequests.FirstOrDefault(p => p.Id == queuedAbsenceRequest.PersonRequest);
				requestValidators.Add(queuedAbsenceRequest.PersonRequest,
					_absenceRequestValidatorProvider.GetValidatorList(
						personRequest, mandatoryValidators));
			}
			return requestValidators;
		}

		private class isNullOrNotNewSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj == null || !(obj.IsPending));
			}
		}

		private class isNullSpecification : Specification<IAbsenceRequest>
		{
			public override bool IsSatisfiedBy(IAbsenceRequest obj)
			{
				return (obj == null);
			}
		}
	}
}
