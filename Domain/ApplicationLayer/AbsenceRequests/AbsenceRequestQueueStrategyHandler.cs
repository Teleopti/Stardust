using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
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
		private readonly IFilterRequestsWithDifferentVersion _filterRequestsWithDifferentVersion;

		public AbsenceRequestQueueStrategyHandler(IAbsenceRequestStrategyProcessor absenceRequestStrategyProcessor,
												  IEventPublisher publisher, INow now,
												  IRequestStrategySettingsReader requestStrategySettingsReader, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
												  IBusinessUnitRepository businessUnitRepository, IBusinessUnitScope businessUnitScope, IPersonRepository personRepository, IUpdatedByScope updatedByScope, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IPersonRequestRepository personRequestRepository, IFilterRequestsWithDifferentVersion filterRequestsWithDifferentVersion)
		{
			_absenceRequestStrategyProcessor = absenceRequestStrategyProcessor;
			_requestStrategySettingsReader = requestStrategySettingsReader;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_businessUnitRepository = businessUnitRepository;
			_businessUnitScope = businessUnitScope;
			_personRepository = personRepository;
			_updatedByScope = updatedByScope;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_filterRequestsWithDifferentVersion = filterRequestsWithDifferentVersion;
			_publisher = publisher;
			_now = now;
		}

		public void Handle(TenantMinuteTickEvent @event)
		{
			IList<IBusinessUnit> businessUnits;
			int windowSize;
			int absenceReqNearFutureTime;
			int absenceReqFarFutureTime;
			IList<IEnumerable<Guid>> listOfAbsenceRequests = new List<IEnumerable<Guid>>();
			IDictionary<Guid, int> reqWithVersion = new Dictionary<Guid, int>();
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
			var initialBu = ((ICurrentBusinessUnit)_businessUnitScope).Current();

			businessUnits.ForEach(businessUnit =>
			{
				_businessUnitScope.OnThisThreadUse(businessUnit);
				
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var now = _now.UtcDateTime();
					var nearFutureThresholdTime = now.AddMinutes(-absenceReqNearFutureTime);
					var farFutureThresholdTime = now.AddMinutes(-absenceReqFarFutureTime);
					var pastThresholdTime = now;

					//include yesterday to deal with timezones
					var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));
					listOfAbsenceRequests = _absenceRequestStrategyProcessor.Get(nearFutureThresholdTime, farFutureThresholdTime,
						pastThresholdTime, initialPeriod, windowSize);

					if (!listOfAbsenceRequests.Any()) return;

					listOfAbsenceRequests = _filterRequestsWithDifferentVersion.Filter(reqWithVersion, listOfAbsenceRequests);

					var sent = _now.UtcDateTime();
					listOfAbsenceRequests.ForEach(absenceRequests =>
					{
						var requests = absenceRequests.ToList();
						var multiAbsenceRequestsEvent = new NewMultiAbsenceRequestsCreatedEvent
						{
							PersonRequestIds = requests,
							Sent = sent
						};
						_publisher.Publish(multiAbsenceRequestsEvent);
						_queuedAbsenceRequestRepository.Send(requests, sent);

						sent = sent.AddSeconds(1);
					});

					uow.PersistAll();
				}
					
				
			});
			_businessUnitScope.OnThisThreadUse(initialBu);
		}
	}
}
