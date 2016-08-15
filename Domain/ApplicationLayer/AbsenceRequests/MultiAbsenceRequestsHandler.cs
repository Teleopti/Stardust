﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class MultiAbsenceRequestsHandler
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestHandler));

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

		public MultiAbsenceRequestsHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, ICurrentScenario scenarioRepository,
			IPersonRequestRepository personRequestRepository, IAbsenceRequestWaitlistProcessor waitlistProcessor,
			IAbsenceRequestProcessor absenceRequestProcessor)
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
				logger.Info("New instance of handler was created");
			}
		}

		public void Handle(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				foreach (var personRequestId in @event.Identities)
				{

					if (_loadDataActions.Any(action => !action.Invoke(personRequestId)))
					{
						return;
					}

					if (shouldUseWaitlisting())
					{
						_waitlistProcessor.ProcessAbsenceRequestWaitlist(unitOfWork, _absenceRequest.Period,
																		 _absenceRequest.Person.WorkflowControlSet);
					}
					else
					{
						_absenceRequestProcessor.ProcessAbsenceRequest(unitOfWork, _absenceRequest, _personRequest);
					}
				}
			}
		}

		private bool shouldUseWaitlisting()
		{
			var workflowControlSet = _absenceRequest.Person.WorkflowControlSet;
			return workflowControlSet != null && workflowControlSet.WaitlistingIsEnabled(_absenceRequest);
		}

		private bool loadDefaultScenario(Guid personRequestId)
		{
			var defaultScenario = _scenarioRepository.Current();
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Using the default scenario named {0}. (Id = {1})", defaultScenario.Description,
									defaultScenario.Id);
			}
			return true;
		}

		private bool checkAbsenceRequest(Guid personRequestId)
		{
			_absenceRequest = _personRequest.Request as IAbsenceRequest;
			if (absenceRequestSpecification.IsSatisfiedBy(_absenceRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat("The found person request is not of type absence request. (Id = {0})",
									  personRequestId);
				}
				return false;
			}
			return true;
		}

		private bool checkPersonRequest(Guid personRequestId)
		{

			_personRequest = _personRequestRepository.Get(personRequestId);
			if (personRequestSpecification.IsSatisfiedBy(_personRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat(
						"No person request found with the supplied Id, or the request is not in New status mode. (Id = {0})",
						personRequestId);
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

		private delegate bool LoadDataAction(Guid personRequestId);
	}
}
