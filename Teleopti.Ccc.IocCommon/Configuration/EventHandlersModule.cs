using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class EventHandlersModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(typeof(IHandleEvent<>).Assembly)
				   .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>)))
				   .As(t => t.GetInterfaces().Where(i => i.GetGenericTypeDefinition() == typeof(IHandleEvent<>)));

			builder.RegisterType<UnitOfWorkTransactionEventSyncronization>().As<IEventSyncronization>().SingleInstance();

			builder.RegisterType<ProjectionChangedEventBuilder>().As<IProjectionChangedEventBuilder>().SingleInstance();
			builder.RegisterType<ScheduleDayReadModelsCreator>().As<IScheduleDayReadModelsCreator>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelsCreator>().As<IPersonScheduleDayReadModelsCreator>().SingleInstance();
			builder.RegisterType<ScheduleDayReadModelComparer>().As<IScheduleDayReadModelComparer>().SingleInstance();
			builder.RegisterType<UpdateScheduleProjectionReadModel>().As<IUpdateScheduleProjectionReadModel>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelPersister>().As<IPersonScheduleDayReadModelPersister>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelFinder>().As<IPersonScheduleDayReadModelFinder>().SingleInstance();
			builder.RegisterType<CommonAgentNameProvider>().As<ICommonAgentNameProvider>().SingleInstance();
			builder.RegisterType<EventTracker>().As<IEventTracker>().SingleInstance();
		}
	}

}