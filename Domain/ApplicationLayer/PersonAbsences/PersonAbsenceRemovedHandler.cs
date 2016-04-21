using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
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
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IAbsenceRequestWaitlistProcessor _waitlistProcessor;
		private readonly IPersonRequestRepository _personRequestRepository;

		public PersonAbsenceRemovedHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, ICurrentScenario scenarioRepository, IAbsenceRequestWaitlistProcessor waitlistProcessor, IPersonRequestRepository personRequestRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_waitlistProcessor = waitlistProcessor;
			_personRequestRepository = personRequestRepository;

			loadDefaultScenario();
		}

		public void Handle(PersonAbsenceRemovedEvent @event)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consuming event for deleted absence for person with Id = {0}.  Absence Period {1} -{2}. (Message timestamp = {3})",
					@event.PersonId, @event.StartDateTime, @event.EndDateTime, @event.Timestamp);
			}

			var personRequest = _personRequestRepository.FindPersonRequestByRequestId(@event.AbsenceRequestId);
			if (personRequest == null)
			{
				return;
			};

			var absenceRequest = personRequest.Request as IAbsenceRequest;
			if (absenceRequest == null || !shouldUseWaitlisting(absenceRequest))
			{
				return;
			}

			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_waitlistProcessor.ProcessAbsenceRequestWaitlist(unitOfWork, absenceRequest.Period, absenceRequest.Person.WorkflowControlSet);
			}
		}

		private static bool shouldUseWaitlisting(IAbsenceRequest absenceRequest)
		{
			var workflowControlSet = absenceRequest.Person.WorkflowControlSet;
			return workflowControlSet != null && workflowControlSet.WaitlistingIsEnabled(absenceRequest);
		}

		private void loadDefaultScenario()
		{
			var defaultScenario = _scenarioRepository.Current();
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Using the default scenario named {0}. (Id = {1})", defaultScenario.Description,
					defaultScenario.Id);
			}
		}
	}
}