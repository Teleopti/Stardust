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

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[RunInterval(5)]
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
												  IBusinessUnitRepository businessUnitRepository, IBusinessUnitScope businessUnitScope, IPersonRepository personRepository, 
												  IUpdatedByScope updatedByScope, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, 
												  IFilterRequestsWithDifferentVersion filterRequestsWithDifferentVersion)
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
			IEnumerable<IBusinessUnit> businessUnits;
			const int windowSize = 2;
			int absenceRequestBulkFrequencyMinutes;
			IDictionary<Guid, int> reqWithVersion = new Dictionary<Guid, int>();
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				businessUnits = _businessUnitRepository.LoadAll();
				var person = _personRepository.Get(SystemUser.Id);
				_updatedByScope.OnThisThreadUse(person);
				
				absenceRequestBulkFrequencyMinutes = _requestStrategySettingsReader.GetIntSetting("AbsenceRequestBulkFrequencyMinutes", 10);
			}

			businessUnits.ForEach(businessUnit =>
			{
			    using (_businessUnitScope.OnThisThreadUse(businessUnit))
			    {
			        var now = _now.UtcDateTime();
			        var thresholdTime = now.AddMinutes(-absenceRequestBulkFrequencyMinutes);
			        var pastThresholdTime = now;

			        //include yesterday to deal with timezones
			        var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)),
			            new DateOnly(now.AddDays(windowSize)));

					var listOfAbsenceRequests = _absenceRequestStrategyProcessor.Get(
						thresholdTime,
						pastThresholdTime, initialPeriod, windowSize);

					if (!listOfAbsenceRequests.Any()) return;

			        listOfAbsenceRequests = _filterRequestsWithDifferentVersion.Filter(reqWithVersion, listOfAbsenceRequests);

			        var sent = _now.UtcDateTime();

					var multiAbsenceRequestsEvents = new List<NewMultiAbsenceRequestsCreatedEvent>();

					listOfAbsenceRequests.ForEach(absenceRequests =>
			        {
						using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
						{
							_queuedAbsenceRequestRepository.Send(absenceRequests.Select(x => x.Id.GetValueOrDefault()).ToList(), sent);
							uow.PersistAll();
						}

						var multiAbsenceRequestsEvent = new NewMultiAbsenceRequestsCreatedEvent
			            {
							PersonRequestIds = absenceRequests.Select(x => x.PersonRequest).ToList(),
							Sent = sent,
							LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
							Ids = absenceRequests.Select(x => x.Id.GetValueOrDefault()).ToList()
						};
						multiAbsenceRequestsEvents.Add(multiAbsenceRequestsEvent);
						
						sent = sent.AddSeconds(1);
			        });

					multiAbsenceRequestsEvents.ForEach(multiAbsenceRequestsEvent => _publisher.Publish(multiAbsenceRequestsEvent));
				}
			});
		}
	}
}
