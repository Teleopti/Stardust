using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{

	public class HardCodedResolver : IResolve
	{
		public object Resolve(Type type)
		{
			// use autofac soon?
			if (type == typeof (IEnumerable<IHandleEvent<ScheduleChangedEvent>>))
				return MakeScheduleChangedHandler();
			if (type == typeof(IEnumerable<IHandleEvent<PersonAbsenceAddedEvent>>))
				return MakeScheduleChangedHandler();
			if (type == typeof (IEnumerable<IHandleEvent<ProjectionChangedEvent>>))
				return new IHandleEvent<ProjectionChangedEvent>[]
					{
						new PersonScheduleDayReadModelHandler(
							new PersonScheduleDayReadModelsCreator(
								new PersonRepository(CurrentUnitOfWork.Make()),
								new NewtonsoftJsonSerializer()),
							new PersonScheduleDayReadModelPersister(
								CurrentUnitOfWork.Make(),
								new DoNotSend(),
								new CurrentDataSource(new CurrentIdentity()))
							),
						new ScheduledResourcesChangedHandler(
							new PersonRepository(CurrentUnitOfWork.Make()),
							new SkillRepository(CurrentUnitOfWork.Make()),
							new ScheduleProjectionReadOnlyRepository(CurrentUnitOfWork.Make()),
							new ScheduledResourcesReadModelUpdater(
								new ScheduledResourcesReadModelStorage(
									CurrentUnitOfWork.Make()), 
									new DoNotSend(), 
									new ControllableEventSyncronization()),
							new PersonSkillProvider(),
							new EventPublisher(this,new CurrentIdentity()))
					};
            Console.WriteLine("Cannot resolve type {0}! Add it manually or consider using autofac!", type);
		    return null;
		}

		private object MakeScheduleChangedHandler()
		{
			return new[]
				{
					new ScheduleChangedHandler(
						new EventPublisher(this,new CurrentIdentity()),
						new ScenarioRepository(CurrentUnitOfWork.Make()),
						new PersonRepository(CurrentUnitOfWork.Make()),
						new ScheduleRepository(CurrentUnitOfWork.Make()),
						new ProjectionChangedEventBuilder())
				};
		}
	}
}