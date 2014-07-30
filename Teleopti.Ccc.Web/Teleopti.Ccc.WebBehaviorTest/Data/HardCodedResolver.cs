using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{

	public class HardCodedResolver : IResolve
	{
		private static IMessageBrokerSender _messageBroker;

		private IMessageBrokerSender messageBroker()
		{
			if (_messageBroker == null)
			{
				var broker = new SignalBroker(MessageFilterManager.Instance, new IConnectionKeepAliveStrategy[] { }, new Time(new Now()));
				broker.ConnectionString = TestSiteConfigurationSetup.URL.ToString();
				broker.StartMessageBroker();
				_messageBroker = broker;
			}
			return _messageBroker;
		}

		public object Resolve(Type type)
		{
			// use autofac soon?
			if (type == typeof (IEnumerable<IHandleEvent<ScheduledResourcesChangedEvent>>))
			{
				var utcTheTime = CurrentTime.Value() == DateTime.MinValue ? DateTime.UtcNow : CurrentTime.Value();
				
				return new[]
					{
						new ScheduleProjectionReadOnlyUpdater(
							new ScheduleProjectionReadOnlyRepository(CurrentUnitOfWork.Make()),
							new EventPublisher(this, new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))),
							new ThisIsNow(utcTheTime)
							)
					};
			}
			if (type == typeof (IEnumerable<IHandleEvent<ScheduleChangedEvent>>))
				return new[]
					{
						makeProjectionChangedEventPublisher(),
						new ScheduleChangedNotifier(messageBroker())
					};
			if (type == typeof (IEnumerable<IHandleEvent<PersonAbsenceAddedEvent>>))
				return new[]
					{
						new ScheduleChangedEventPublisher(new EventPublisher(this, new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))
					};
			if (type == typeof(IEnumerable<IHandleEvent<FullDayAbsenceAddedEvent>>))
				return new[]
					{
						new ScheduleChangedEventPublisher(new EventPublisher(this, new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))
					};
			if (type == typeof (IEnumerable<IHandleEvent<ProjectionChangedEvent>>))
				return new IHandleEvent<ProjectionChangedEvent>[]
					{
						new PersonScheduleDayReadModelUpdater(
							new PersonScheduleDayReadModelsCreator(
								new PersonRepository(CurrentUnitOfWork.Make()),
								new NewtonsoftJsonSerializer()),
							new PersonScheduleDayReadModelPersister(
								CurrentUnitOfWork.Make(),
								messageBroker(),
								new CurrentDataSource(new CurrentIdentity())),
								null
							),
						new ScheduledResourcesChangedHandler(
							new PersonRepository(CurrentUnitOfWork.Make()),
							new SkillRepository(CurrentUnitOfWork.Make()),
							new ScheduleProjectionReadOnlyRepository(CurrentUnitOfWork.Make()),
							new ScheduledResourcesReadModelUpdater(
								new ScheduledResourcesReadModelStorage(
									CurrentUnitOfWork.Make()
									),
								messageBroker(),
								new UnitOfWorkTransactionEventSyncronization(CurrentUnitOfWork.Make())),
							new PersonSkillProvider(),
							new EventPublisher(this, new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))
					};
            Console.WriteLine("Cannot resolve type {0}! Add it manually or consider using autofac!", type);
		    return null;
		}

		private object makeProjectionChangedEventPublisher()
		{
			return new ProjectionChangedEventPublisher(
					 new EventPublisher(this, new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))),
					 new ScenarioRepository(CurrentUnitOfWork.Make()),
					 new PersonRepository(CurrentUnitOfWork.Make()),
					 new ScheduleRepository(CurrentUnitOfWork.Make()),
					 new ProjectionChangedEventBuilder()
					 );
		}
	}
}