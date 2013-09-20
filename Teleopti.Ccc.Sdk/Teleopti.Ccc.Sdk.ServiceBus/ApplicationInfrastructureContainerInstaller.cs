using System;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ApplicationInfrastructureContainerInstaller : Module
    {
    	[ThreadStatic] private static IJobResultFeedback jobResultFeedback;

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BusStartup>().As<IServiceBusAware>().SingleInstance();
			builder.Register(c => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging)
				.As<IMessageBroker>()
				.As<IMessageBrokerSender>()
				.As<IMessageBrokerListener>()
				.ExternallyOwned();
			builder.Register(c => StateHolderReader.Instance.StateReader.ApplicationScopeData).As<IApplicationData>().ExternallyOwned();
			builder.Register(c => UnitOfWorkFactoryContainer.Current).As<ICurrentUnitOfWorkFactory>().ExternallyOwned();
			builder.RegisterType<CurrentUnitOfWork>().As<ICurrentUnitOfWork>().SingleInstance();
			builder.RegisterType<CurrentDataSource>().As<ICurrentDataSource>().SingleInstance();
			builder.Register(getThreadJobResultFeedback).As<IJobResultFeedback>().ExternallyOwned();
			builder.RegisterType<SendPushMessageWhenRootAlteredService>().As<ISendPushMessageWhenRootAlteredService>().InstancePerDependency();
			builder.RegisterType<RepositoryFactory>().As<IRepositoryFactory>().SingleInstance();
			builder.RegisterType<InternalServiceBusSender>().As<IServiceBusSender>().SingleInstance();
			builder.RegisterType<GroupingReadOnlyRepository>().As<IGroupingReadOnlyRepository>().SingleInstance();
			builder.RegisterType<ScheduledResourcesReadModelStorage>()
				.As<IScheduledResourcesReadModelPersister>()
				.As<IScheduledResourcesReadModelReader>()
				.SingleInstance();
		}

    	private static IJobResultFeedback getThreadJobResultFeedback(IComponentContext componentContext)
    	{
    		return jobResultFeedback ?? (jobResultFeedback = new JobResultFeedback(componentContext.Resolve<ICurrentUnitOfWorkFactory>()));
    	}
    }
}