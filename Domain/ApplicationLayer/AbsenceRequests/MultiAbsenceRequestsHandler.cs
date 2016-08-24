using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
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
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonRequestRepository _personRequestRepository;
		private List<IPersonRequest> _personRequests;
		private readonly IMultiAbsenceRequestProcessor _absenceRequestProcessor;
		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;



		public MultiAbsenceRequestsHandler(ICurrentScenario currentScenario, IPersonRequestRepository personRequestRepository,
			IMultiAbsenceRequestProcessor absenceRequestProcessor, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentScenario = currentScenario;
			_personRequestRepository = personRequestRepository;
			_absenceRequestProcessor = absenceRequestProcessor;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;

			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			_currentScenario.Current();
			checkPersonRequest(@event.PersonRequestIds);
			_absenceRequestProcessor.ProcessAbsenceRequest(_currentUnitOfWorkFactory.Current().CurrentUnitOfWork(), _personRequests);
		}


		private void checkPersonRequest(List<Guid> personRequestIds)
		{
			_personRequests = new List<IPersonRequest>();

			var personRequests = _personRequestRepository.Find(personRequestIds);
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
				else if (absenceRequestSpecification.IsSatisfiedBy((IAbsenceRequest)personRequest.Request))
				{
					if (logger.IsWarnEnabled)
					{
						logger.WarnFormat("The found person request is not of type absence request. (Id = {0})",
										  personRequest.Id);
					}
				}
				else
					_personRequests.Add(personRequest);
			}
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
	}
}
