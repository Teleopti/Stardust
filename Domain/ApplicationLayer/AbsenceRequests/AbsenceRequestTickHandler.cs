﻿using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
	public class AbsenceRequestTickHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
	{
		private readonly IAbsenceRequestStrategyProcessor _absenceRequestStrategyProcessor;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IEventPublisher _publisher;
		private readonly INow _now;
		private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IBusinessUnitScope _businessUnitScope;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IPersonRepository _personRepository;
		private readonly DataSourceState _dataSourceState;

		public AbsenceRequestTickHandler(IAbsenceRequestStrategyProcessor absenceRequestStrategyProcessor,
			IEventPublisher publisher, INow now, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IRequestStrategySettingsReader requestStrategySettingsReader, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IBusinessUnitRepository businessUnitRepository, IBusinessUnitScope businessUnitScope, ICurrentDataSource currentDataSource, IDataSourceScope dataSourceScope, IPersonRepository personRepository, DataSourceState dataSourceState)
		{
			_absenceRequestStrategyProcessor = absenceRequestStrategyProcessor;
			_publisher = publisher;
			_now = now;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_requestStrategySettingsReader = requestStrategySettingsReader;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_businessUnitRepository = businessUnitRepository;
			_businessUnitScope = businessUnitScope;
			_currentDataSource = currentDataSource;
			_dataSourceScope = dataSourceScope;
			_personRepository = personRepository;
			_dataSourceState = dataSourceState;
		}

		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			using (_dataSourceScope.OnThisThreadUse(_currentDataSource.Current()))
			{
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var businessUnits = _businessUnitRepository.LoadAll();
					var person = _personRepository.Get(SystemUser.Id);

					businessUnits.ForEach(businessUnit =>
					{
						Thread.CurrentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity(person.Name.FirstName, _dataSourceState.Get(), businessUnit, null,""), person);

						_businessUnitScope.OnThisThreadUse(businessUnit);
						var nearFuture = _requestStrategySettingsReader.GetIntSetting("AbsenceNearFuture", 3);
						var absenceReqNearFutureTime = _requestStrategySettingsReader.GetIntSetting("AbsenceNearFutureTime", 20);
						var absenceReqFarFutureTime = _requestStrategySettingsReader.GetIntSetting("AbsenceFarFutureTime", 60);

						var now = _now.UtcDateTime();
						var nearFutureInterval = _now.UtcDateTime().AddMinutes(absenceReqNearFutureTime * -1);
						var farFutureInterval = _now.UtcDateTime().AddMinutes(absenceReqFarFutureTime * -1);

						var absenceRequests = _absenceRequestStrategyProcessor.Get(nearFutureInterval, farFutureInterval,
							new DateTimePeriod(now.Date, now.Date.AddDays(nearFuture))).ToList();
						if (!absenceRequests.Any()) return;
						var multiAbsenceRequestsEvent = new NewMultiAbsenceRequestsCreatedEvent()
						{
							PersonRequestIds = absenceRequests.ToList()
						};
						_publisher.Publish(multiAbsenceRequestsEvent);
						_queuedAbsenceRequestRepository.Remove(absenceRequests);
						uow.PersistAll();
					});
					
				}
			}
			//
			{
				
				//
			}
		}
	}

}
