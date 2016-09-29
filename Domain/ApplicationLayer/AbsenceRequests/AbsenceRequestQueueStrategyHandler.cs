using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
	public class AbsenceRequestQueueStrategyHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
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
			int windowSize;
			int absenceReqNearFutureTime;
			int absenceReqFarFutureTime;

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				businessUnits = _businessUnitRepository.LoadAll();
				var person = _personRepository.Get(SystemUser.Id);
				_updatedByScope.OnThisThreadUse(person);

				windowSize = _requestStrategySettingsReader.GetIntSetting("AbsenceNearFuture", 3);
				absenceReqNearFutureTime = _requestStrategySettingsReader.GetIntSetting("AbsenceNearFutureTime", 20);
				absenceReqFarFutureTime = _requestStrategySettingsReader.GetIntSetting("AbsenceFarFutureTime", 60);
				var bulkRequestTimeoutMinutes = _requestStrategySettingsReader.GetIntSetting("BulkRequestTimeoutMinutes", 90);

				_queuedAbsenceRequestRepository.CheckAndUpdateSent(bulkRequestTimeoutMinutes);
				uow.PersistAll();
			}

			businessUnits.ForEach(businessUnit =>
			{
				_businessUnitScope.OnThisThreadUse(businessUnit);
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var now = _now.UtcDateTime();
					var nearFutureThresholdTime = _now.UtcDateTime().AddMinutes(-absenceReqNearFutureTime);
					var farFutureThresholdTime = _now.UtcDateTime().AddMinutes(-absenceReqFarFutureTime);
					var pastThresholdTime = _now.UtcDateTime();

					//include yesterday to deal with timezones
					var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));
					var listOfAbsenceRequests = _absenceRequestStrategyProcessor.Get(nearFutureThresholdTime, farFutureThresholdTime, pastThresholdTime, initialPeriod, windowSize);
					if (!listOfAbsenceRequests.Any()) return;

					listOfAbsenceRequests.ForEach(absenceRequests =>
					{
						var sent = DateTime.UtcNow;

						var multiAbsenceRequestsEvent = new NewMultiAbsenceRequestsCreatedEvent()
						{
							PersonRequestIds = absenceRequests.ToList(),
							Sent = sent
						};
						_publisher.Publish(multiAbsenceRequestsEvent);
						_queuedAbsenceRequestRepository.Send(absenceRequests.ToList(), sent);
						Thread.Sleep(1000); //Sleep 1 second to make unique timestamps
					});

					uow.PersistAll();
				}

			});
		}
	}
}
