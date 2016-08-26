using System.Collections.Generic;
using System.Linq;
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
		private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IBusinessUnitScope _businessUnitScope;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IPersonRepository _personRepository;
		private readonly DataSourceState _dataSourceState;
		private readonly IEventPublisher _publisher;
		private readonly INow _now;

		public AbsenceRequestTickHandler(IAbsenceRequestStrategyProcessor absenceRequestStrategyProcessor,
										 IEventPublisher publisher, INow now, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository,
										 IRequestStrategySettingsReader requestStrategySettingsReader, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
										 IBusinessUnitRepository businessUnitRepository, IBusinessUnitScope businessUnitScope, ICurrentDataSource currentDataSource,
										 IDataSourceScope dataSourceScope, IPersonRepository personRepository, DataSourceState dataSourceState)
		{
			_absenceRequestStrategyProcessor = absenceRequestStrategyProcessor;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_requestStrategySettingsReader = requestStrategySettingsReader;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_businessUnitRepository = businessUnitRepository;
			_businessUnitScope = businessUnitScope;
			_currentDataSource = currentDataSource;
			_dataSourceScope = dataSourceScope;
			_personRepository = personRepository;
			_dataSourceState = dataSourceState;
			_publisher = publisher;
			_now = now;
		}

		public void Handle(TenantMinuteTickEvent @event)
		{
			using (_dataSourceScope.OnThisThreadUse(_currentDataSource.Current()))
			{
				IList<IBusinessUnit> businessUnits;
				IPerson person;
				Person tmpPerson = new Person();
				using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					businessUnits = _businessUnitRepository.LoadAll();
					person = _personRepository.Get(SystemUser.Id);
					tmpPerson.Name = person.Name;
				}

				businessUnits.ForEach(businessUnit =>
				{
					Thread.CurrentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity(tmpPerson.Name.FirstName, _dataSourceState.Get(), businessUnit, null, ""), tmpPerson);

					_businessUnitScope.OnThisThreadUse(businessUnit);
					using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						var nearFuture = _requestStrategySettingsReader.GetIntSetting("AbsenceNearFuture", 3);
						var absenceReqNearFutureTime = _requestStrategySettingsReader.GetIntSetting("AbsenceNearFutureTime", 20);
						var absenceReqFarFutureTime = _requestStrategySettingsReader.GetIntSetting("AbsenceFarFutureTime", 60);

						var now = _now.UtcDateTime();
						var nearFutureInterval = _now.UtcDateTime().AddMinutes(absenceReqNearFutureTime*-1);
						var farFutureInterval = _now.UtcDateTime().AddMinutes(absenceReqFarFutureTime*-1);

						//include yesterday to deal with timezones
						var nearFuturePeriod = new DateTimePeriod(now.Date.AddDays(-1), now.Date.AddDays(nearFuture));
						var listOfAbsenceRequests = _absenceRequestStrategyProcessor.Get(nearFutureInterval, farFutureInterval, nearFuturePeriod, nearFuture);
						if (!listOfAbsenceRequests.Any()) return;
						listOfAbsenceRequests.ForEach(absenceRequests =>
						{
							var multiAbsenceRequestsEvent = new NewMultiAbsenceRequestsCreatedEvent()
							{
								PersonRequestIds = absenceRequests.ToList()
							};
							_publisher.Publish(multiAbsenceRequestsEvent);
							_queuedAbsenceRequestRepository.Remove(absenceRequests);
						});

						uow.PersistAll();
					}

				});

			}
		}

	}
}
