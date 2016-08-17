using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class NewAbsenceRequestUseMultiHandler : INewAbsenceRequestHandler, IHandleEvent<NewAbsenceRequestCreatedEvent>, IRunOnStardust
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestUseMultiHandler));

		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAbsenceRequestWaitlistProcessor _waitlistProcessor;
		private readonly IAbsenceRequestProcessor _absenceRequestProcessor;


		public NewAbsenceRequestUseMultiHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, ICurrentScenario scenarioRepository,
			IPersonRequestRepository personRequestRepository, IAbsenceRequestWaitlistProcessor waitlistProcessor,
			IAbsenceRequestProcessor absenceRequestProcessor)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRequestRepository = personRequestRepository;
			_waitlistProcessor = waitlistProcessor;
			_absenceRequestProcessor = absenceRequestProcessor;
		}

		[AsSystem]
		public void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			
		}

	}
}
