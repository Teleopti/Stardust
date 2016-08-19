using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
	public class NewAbsenceRequestUseMultiHandler : INewAbsenceRequestHandler, IHandleEvent<NewAbsenceRequestCreatedEvent>, IRunOnStardust
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestUseMultiHandler));
		
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly IConfigReader _configReader;
		private readonly DataSourceState _dataSourceState;
		private readonly IDataSourceScope _dataSourceScope;

		private readonly IList<LoadDataAction> _loadDataActions;
		private IPersonRequest _personRequest;

		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();

		private delegate bool LoadDataAction(NewAbsenceRequestCreatedEvent @event);

		public NewAbsenceRequestUseMultiHandler(IPersonRequestRepository personRequestRepository, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, 
			IEventPublisher eventPublisher, IConfigReader configReader, DataSourceState dataSourceState,
			IDataSourceScope dataSourceScope )
		{
			_personRequestRepository = personRequestRepository;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_eventPublisher = eventPublisher;
			_configReader = configReader;
			_dataSourceState = dataSourceState;
			_dataSourceScope = dataSourceScope;

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
				// before call we need to set up a person as it is logged on
				using (var uow =_dataSourceState.Get().Application.CreateAndOpenUnitOfWork())
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
					var requestWithOverlappingPeriod = _queuedAbsenceRequestRepository.Find(_personRequest.Request.Period);

					int numberOfAbsenceRequestsToBulkProcess;
					int.TryParse(_configReader.AppConfig("NumberOfAbsenceRequestsToBulkProcess"), out numberOfAbsenceRequestsToBulkProcess);

					if (requestWithOverlappingPeriod.Count >= numberOfAbsenceRequestsToBulkProcess)
					{
						var Ids = new List<Guid>();
						foreach (var req in requestWithOverlappingPeriod)
						{
							Ids.Add(req.PersonRequest);

							_queuedAbsenceRequestRepository.Remove(req);
							uow.PersistAll();
						}
						var multiRequestEvent = new NewMultiAbsenceRequestsCreatedEvent()
						{
							PersonRequestIds = Ids
						};
						_eventPublisher.Publish(multiRequestEvent);

					}
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
