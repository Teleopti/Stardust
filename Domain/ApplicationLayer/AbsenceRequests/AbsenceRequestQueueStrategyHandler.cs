using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
	public class AbsenceRequestQueueStrategyHandler : IHandleEvent<TenantMinuteTickEvent>, IHandleEvent<PersonRequestProcessedEvent>, IRunOnHangfire
	{
		private readonly IAbsenceRequestStrategyProcessor _absenceRequestStrategyProcessor;
		private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IBusinessUnitScope _businessUnitScope;
		private readonly IPersonRepository _personRepository;
		private readonly IEventPublisher _publisher;
		private readonly INow _now;
		private readonly IUpdatedByScope _updatedByScope;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;

		public AbsenceRequestQueueStrategyHandler(IAbsenceRequestStrategyProcessor absenceRequestStrategyProcessor,
										 IEventPublisher publisher, INow now,
										 IRequestStrategySettingsReader requestStrategySettingsReader, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
										 IBusinessUnitRepository businessUnitRepository, IBusinessUnitScope businessUnitScope, IPersonRepository personRepository, IUpdatedByScope updatedByScope, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository)
		{
			_absenceRequestStrategyProcessor = absenceRequestStrategyProcessor;
			_requestStrategySettingsReader = requestStrategySettingsReader;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_businessUnitRepository = businessUnitRepository;
			_businessUnitScope = businessUnitScope;
			_personRepository = personRepository;
			_updatedByScope = updatedByScope;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_publisher = publisher;
			_now = now;
		}

		public void Handle(TenantMinuteTickEvent @event)
		{
			IList<IBusinessUnit> businessUnits;
			int nearFuture;
			int absenceReqNearFutureTime;
			int absenceReqFarFutureTime;

			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				businessUnits = _businessUnitRepository.LoadAll();
				var person = _personRepository.Get(SystemUser.Id);
				_updatedByScope.OnThisThreadUse(person);

				nearFuture = _requestStrategySettingsReader.GetIntSetting("AbsenceNearFuture", 3);
				absenceReqNearFutureTime = _requestStrategySettingsReader.GetIntSetting("AbsenceNearFutureTime", 20);
				absenceReqFarFutureTime = _requestStrategySettingsReader.GetIntSetting("AbsenceFarFutureTime", 60);
			}

			businessUnits.ForEach(businessUnit =>
			{
				_businessUnitScope.OnThisThreadUse(businessUnit);
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var now = _now.UtcDateTime();
					var nearFutureInterval = _now.UtcDateTime().AddMinutes(absenceReqNearFutureTime*-1);
					var farFutureInterval = _now.UtcDateTime().AddMinutes(absenceReqFarFutureTime*-1);

					//include yesterday to deal with timezones
					var nearFuturePeriod = new DateTimePeriod(now.Date.AddDays(-1).Utc(), now.Date.AddDays(nearFuture).Utc());
					var listOfAbsenceRequests = _absenceRequestStrategyProcessor.Get(nearFutureInterval, farFutureInterval, nearFuturePeriod, nearFuture);
					if (!listOfAbsenceRequests.Any()) return;
					listOfAbsenceRequests.ForEach(absenceRequests =>
					{
						var multiAbsenceRequestsEvent = new NewMultiAbsenceRequestsCreatedEvent()
						{
							PersonRequestIds = absenceRequests.ToList()
						};
						_publisher.Publish(multiAbsenceRequestsEvent);
					
					});

					uow.PersistAll();
				}

			});

		}

		public void Handle(PersonRequestProcessedEvent @event)
		{
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_queuedAbsenceRequestRepository.Remove(new List<Guid>() {@event.PersonRequestId});
			}
		}
	}
}
