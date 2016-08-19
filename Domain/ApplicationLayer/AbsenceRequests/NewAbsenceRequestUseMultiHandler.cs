using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
	public class NewAbsenceRequestUseMultiHandler : INewAbsenceRequestHandler, IHandleEvent<NewAbsenceRequestCreatedEvent>, IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestUseMultiHandler));
		
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		private readonly IList<LoadDataAction> _loadDataActions;
		private IPersonRequest _personRequest;

		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();

		private delegate bool LoadDataAction(NewAbsenceRequestCreatedEvent @event);

		public NewAbsenceRequestUseMultiHandler(IPersonRequestRepository personRequestRepository, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository,
			IDataSourceScope dataSourceScope, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory )
		{
			_personRequestRepository = personRequestRepository;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_dataSourceScope = dataSourceScope;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;

			_loadDataActions = new List<LoadDataAction>
			{
				checkPersonRequest,
				checkAbsenceRequest
			};
		}


		public void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			using (_dataSourceScope.OnThisThreadUse(@event.LogOnDatasource))
			{
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					if (_loadDataActions.Any(action => !action.Invoke(@event)))
					{
						return;
					}

					var queuedAbsenceRequest = new QueuedAbsenceRequest()
					{
						PersonRequest = _personRequest.Id.Value,
						Created = _personRequest.CreatedOn.Value,
						StartDateTime = _personRequest.Request.Period.StartDateTime,
						EndDateTime = _personRequest.Request.Period.EndDateTime,
					};
					_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
					uow.PersistAll();
				}
			}

		}

		private bool checkAbsenceRequest(NewAbsenceRequestCreatedEvent @event)
		{
			var req  = _personRequest.Request as IAbsenceRequest;
			if (absenceRequestSpecification.IsSatisfiedBy(req))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat("The found person request is not of type absence request. (Id = {0})",
									  @event.PersonRequestId);
				}
				return false;
			}
			return true;
		}

		private bool checkPersonRequest(NewAbsenceRequestCreatedEvent @event)
		{
			_personRequest = _personRequestRepository.Get(@event.PersonRequestId);
			if (personRequestSpecification.IsSatisfiedBy(_personRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat(
						"No person request found with the supplied Id, or the request is not in New status mode. (Id = {0})",
						@event.PersonRequestId);
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

	}
}
