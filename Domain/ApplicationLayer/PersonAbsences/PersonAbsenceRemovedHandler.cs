using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonAbsences
{

	//ROBTODO: this will need to be moved to stardust.
#pragma warning disable 618
	public class PersonAbsenceRemovedHandler : IHandleEvent<PersonAbsenceRemovedEvent>, IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(PersonAbsenceRemovedHandler));

		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAbsenceRequestWaitlistProcessor _waitlistProcessor;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;

		public PersonAbsenceRemovedHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IAbsenceRequestWaitlistProcessor waitlistProcessor, IPersonRequestRepository personRequestRepository, IPersonRequestCheckAuthorization personRequestCheckAuthorization)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_waitlistProcessor = waitlistProcessor;
			_personRequestRepository = personRequestRepository;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
		}

		public void Handle(PersonAbsenceRemovedEvent @event)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Consuming event for deleted absence for person with Id = {0}.  Absence Period {1} -{2}. (Message timestamp = {3})",
					@event.PersonId, @event.StartDateTime, @event.EndDateTime, @event.Timestamp);
			}

			var personRequest = _personRequestRepository.FindPersonRequestByRequestId(@event.AbsenceRequestId);
			var absenceRequest = personRequest?.Request as IAbsenceRequest;
			if (absenceRequest == null || !shouldProcessPersonRequest(personRequest))
			{
				return;
			}
			
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				cancelAbsenceRequest(absenceRequest, personRequest);

				if (shouldUseWaitlisting(absenceRequest))
				{
					_waitlistProcessor.ProcessAbsenceRequestWaitlist(unitOfWork, absenceRequest.Period, absenceRequest.Person.WorkflowControlSet);
				}
			}
		}

		private static bool shouldProcessPersonRequest(IPersonRequest personRequest)
		{
			return (personRequest.IsApproved || personRequest.IsCancelled);
		}

		private void cancelAbsenceRequest(IAbsenceRequest absenceRequest, IPersonRequest personRequest)
		{
			if (personRequest.IsCancelled)
			{
				return;
			}

			if (absenceRequest.PersonAbsences.IsEmpty())
			{
				personRequest?.Cancel(_personRequestCheckAuthorization);
			}
		}

		private static bool shouldUseWaitlisting(IAbsenceRequest absenceRequest)
		{
			var workflowControlSet = absenceRequest.Person.WorkflowControlSet;
			return workflowControlSet != null && workflowControlSet.WaitlistingIsEnabled(absenceRequest);
		}

	}
}