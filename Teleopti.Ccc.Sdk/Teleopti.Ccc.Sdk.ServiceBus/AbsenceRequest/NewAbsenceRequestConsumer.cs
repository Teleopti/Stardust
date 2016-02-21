using System.Collections.Generic;
using System.Linq;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class NewAbsenceRequestConsumer : ConsumerOf<NewAbsenceRequestCreated>
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestConsumer));

		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IPersonRequestRepository _personRequestRepository;

		private IPersonRequest _personRequest;
		private IAbsenceRequest _absenceRequest;
		private readonly IAbsenceRequestWaitlistProcessor _waitlistProcessor;
		private readonly IAbsenceRequestProcessor _absenceRequestProcessor;

		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();

		private readonly IList<LoadDataAction> _loadDataActions;

		public NewAbsenceRequestConsumer(ICurrentUnitOfWorkFactory unitOfWorkFactory, ICurrentScenario scenarioRepository, IPersonRequestRepository personRequestRepository, IAbsenceRequestWaitlistProcessor waitlistProcessor, IAbsenceRequestProcessor absenceRequestProcessor)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRequestRepository = personRequestRepository;
			_waitlistProcessor = waitlistProcessor;
			_absenceRequestProcessor = absenceRequestProcessor;

			_loadDataActions = new List<LoadDataAction>
                                   {
                                       checkPersonRequest,
                                       checkAbsenceRequest,
                                       loadDefaultScenario
                                   };
			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of consumer was created");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Consume(NewAbsenceRequestCreated message)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consuming message for person request with Id = {0}. (Message timestamp = {1})",
								   message.PersonRequestId, message.Timestamp);
			}

			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				if (_loadDataActions.Any(action => !action.Invoke(message)))
				{
					return;
				}

				if (shouldUseWaitlisting())
				{
					_waitlistProcessor.ProcessAbsenceRequestWaitlist (unitOfWork, _absenceRequest.Period, _absenceRequest.Person.WorkflowControlSet);
				}
				else
				{
					_absenceRequestProcessor.ProcessAbsenceRequest(unitOfWork, _absenceRequest, _personRequest);	
				}
			}
		}

		private bool shouldUseWaitlisting()
		{
			var workflowControlSet = _absenceRequest.Person.WorkflowControlSet;
			return workflowControlSet != null && workflowControlSet.WaitlistingIsEnabled(_absenceRequest);
		}
		
		private bool loadDefaultScenario(NewAbsenceRequestCreated message)
		{
			var defaultScenario = _scenarioRepository.Current();
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Using the default scenario named {0}. (Id = {1})", defaultScenario.Description,
								   defaultScenario.Id);
			}
			return true;
		}

		private bool checkAbsenceRequest(NewAbsenceRequestCreated message)
		{
			_absenceRequest = _personRequest.Request as IAbsenceRequest;
			if (absenceRequestSpecification.IsSatisfiedBy(_absenceRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat("The found person request is not of type absence request. (Id = {0})",
									  message.PersonRequestId);
				}
				return false;
			}
			return true;
		}

		private bool checkPersonRequest(NewAbsenceRequestCreated message)
		{

			_personRequest = _personRequestRepository.Get(message.PersonRequestId);
			if (personRequestSpecification.IsSatisfiedBy(_personRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat(
						"No person request found with the supplied Id, or the request is not in New status mode. (Id = {0})",
						message.PersonRequestId);
				}
				return false;
			}
			return true;
		}

		private class isNullOrNotNewSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj == null || !obj.IsNew);
			}
		}

		private class isNullSpecification : Specification<IAbsenceRequest>
		{
			public override bool IsSatisfiedBy(IAbsenceRequest obj)
			{
				return (obj == null);
			}
		}

		private delegate bool LoadDataAction(NewAbsenceRequestCreated message);
	}

}
