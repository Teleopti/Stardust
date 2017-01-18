﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
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

		public MultiAbsenceRequestsHandler(IPersonRequestRepository personRequestRepository,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IStardustJobFeedback stardustJobFeedback, IWorkflowControlSetRepository workflowControlSetRepository,
			IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IMultiAbsenceRequestsUpdater multiAbsenceRequestsUpdater, IAbsenceRequestValidatorProvider absenceRequestValidatorProvider)
		{
			_personRequestRepository = personRequestRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_feedback = stardustJobFeedback;
			_workflowControlSetRepository = workflowControlSetRepository;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_multiAbsenceRequestsUpdater = multiAbsenceRequestsUpdater;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
		}

		[AsSystem]
		public virtual void Handle(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			_feedback.SendProgress($"Received {@event.PersonRequestIds.Count} Absence Requests.");

			var personRequests = checkPersonRequest(@event);
			
			_feedback.SendProgress($"Done Checking Person Requests. {personRequests.Count} will be processed.");
			if (!personRequests.IsNullOrEmpty())
			{
				IDictionary<Guid, IEnumerable<IAbsenceRequestValidator>> requestValidators;
				using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					requestValidators = getMandatoryValidators(personRequests);
				}
				_multiAbsenceRequestsUpdater.UpdateAbsenceRequest(personRequests, requestValidators);
			}
				
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_queuedAbsenceRequestRepository.Remove(@event.Sent);
				uow.PersistAll();
			}
		}

		private List<Guid> checkPersonRequest(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			DateTime min = DateTime.MaxValue;
			DateTime max = DateTime.MinValue;
			var requests = new List<IPersonRequest>();
			var atleastOneWaitlistedWfc = false;

			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var wfcs = _workflowControlSetRepository.LoadAll();
				if (wfcs.Any(x => x.AbsenceRequestWaitlistEnabled))
				{
					atleastOneWaitlistedWfc = true;
					var queuedRequests = _queuedAbsenceRequestRepository.LoadAll().Where(x => x.Sent == @event.Sent);

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
						string warning = $"No person request found with the supplied Id or the request is not in pending status mode. (Id = {personRequest.Id})";
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
					var waitListIds = _personRequestRepository.GetWaitlistRequests(period).ToList();
					requests.AddRange(_personRequestRepository.Find(waitListIds));
					_feedback.SendProgress($"Picked up {waitListIds.Count} waitlisted requests in period {period}.");
				}
				
			}
			return requests.Select(x => x.Id.GetValueOrDefault()).ToList();
		}

		private IDictionary<Guid, IEnumerable<IAbsenceRequestValidator>> getMandatoryValidators(IEnumerable<Guid> personRequestIds)
		{
			var requestValidators = new Dictionary<Guid, IEnumerable<IAbsenceRequestValidator>>();
			var queuedAbsenceRequests = _queuedAbsenceRequestRepository.FindByPersonRequestIds(personRequestIds);
			foreach (var queuedAbsenceRequest in queuedAbsenceRequests)
			{
				var mandatoryValidators = queuedAbsenceRequest.MandatoryValidators;
				if (requestValidators.ContainsKey(queuedAbsenceRequest.PersonRequest))
					continue;
				requestValidators.Add(queuedAbsenceRequest.PersonRequest,
					_absenceRequestValidatorProvider.GetValidatorList(mandatoryValidators));
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
