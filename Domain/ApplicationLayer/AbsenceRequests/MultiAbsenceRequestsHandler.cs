using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
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
		private static readonly ILog logger = LogManager.GetLogger(typeof(MultiAbsenceRequestsHandler));
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IMultiAbsenceRequestProcessor _absenceRequestProcessor;
		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IStardustJobFeedback _feedback;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;


		public MultiAbsenceRequestsHandler(IPersonRequestRepository personRequestRepository,
			IMultiAbsenceRequestProcessor absenceRequestProcessor, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
			IStardustJobFeedback stardustJobFeedback, IWorkflowControlSetRepository workflowControlSetRepository, 
			IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IPersonRepository personRepository, ISkillRepository skillRepository)
		{
			_personRequestRepository = personRequestRepository;
			_absenceRequestProcessor = absenceRequestProcessor;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_feedback = stardustJobFeedback;
			_workflowControlSetRepository = workflowControlSetRepository;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
		}

		[AsSystem]
		public virtual void Handle(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			var personRequests = checkPersonRequest(@event);
			
			_feedback.SendProgress?.Invoke("Done Checking Person Requests.");
			if (!personRequests.IsNullOrEmpty())
				_absenceRequestProcessor.ProcessAbsenceRequest(personRequests);

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_queuedAbsenceRequestRepository.Remove(@event.Sent);
				uow.PersistAll();
			}
		}

		private List<IPersonRequest> checkPersonRequest(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			DateTime min = DateTime.MaxValue;
			DateTime max = DateTime.MinValue;
			var requests = new List<IPersonRequest>();

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var wfcs = _workflowControlSetRepository.LoadAll();
				if (wfcs.Any(x => x.AbsenceRequestWaitlistEnabled))
				{
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
						if (logger.IsWarnEnabled)
						{
							logger.WarnFormat(
								"No person request found with the supplied Id, or the request is not in New status mode. (Id = {0})",
								personRequest.Id);
						}
					}
					else if (absenceRequestSpecification.IsSatisfiedBy((IAbsenceRequest) personRequest.Request))
					{
						if (logger.IsWarnEnabled)
						{
							logger.WarnFormat("The found person request is not of type absence request. (Id = {0})",
											  personRequest.Id);
						}
					}
					else
					{
						personRequest.Pending();
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

				if (max > min)
				{
					DateTimePeriod period = new DateTimePeriod(min.Utc(), max.Utc());
					var waitListIds = _personRequestRepository.GetWaitlistRequests(period).ToList();
					requests.AddRange(_personRequestRepository.Find(waitListIds));
				}

				_personRepository.FindPeople(personRequests.Select(x => x.Person.Id.GetValueOrDefault()));
				var skills = _skillRepository.LoadAll();

				foreach (var skill in skills)
				{
					_skillRepository.LoadSkill(skill);
				}
				uow.PersistAll();
			}
			return requests;
		}

		private class isNullOrNotNewSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj == null || !(obj.IsNew || obj.IsPending));
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
