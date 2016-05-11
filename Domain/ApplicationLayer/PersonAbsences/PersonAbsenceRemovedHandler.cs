using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonAbsences
{
	[UseOnToggle(Toggles.Wfm_Requests_Cancel_37741)]
#pragma warning disable 618
	public class PersonAbsenceRemovedHandler : IHandleEvent<PersonAbsenceRemovedEvent>, IRunOnStardust
#pragma warning restore 618
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(PersonAbsenceRemovedHandler));

		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAbsenceRequestWaitlistProcessor _waitlistProcessor;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAbsenceRequestCancelService _absenceRequestCancelService;

		public PersonAbsenceRemovedHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IAbsenceRequestWaitlistProcessor waitlistProcessor, IPersonRequestRepository personRequestRepository, IAbsenceRequestCancelService absenceRequestCancelService)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_waitlistProcessor = waitlistProcessor;
			_personRequestRepository = personRequestRepository;
			_absenceRequestCancelService = absenceRequestCancelService;
		}

		[AsSystem]
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
				_absenceRequestCancelService.CancelAbsenceRequest(absenceRequest);

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

		
		private static bool shouldUseWaitlisting(IAbsenceRequest absenceRequest)
		{
			var workflowControlSet = absenceRequest.Person.WorkflowControlSet;
			return workflowControlSet != null && workflowControlSet.WaitlistingIsEnabled(absenceRequest);
		}

	}
}