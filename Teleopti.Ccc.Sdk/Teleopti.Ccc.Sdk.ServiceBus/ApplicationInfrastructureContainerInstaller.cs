using System;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
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
			builder.Register(c => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging).As<IMessageBroker>().ExternallyOwned();
			builder.Register(c => StateHolderReader.Instance.StateReader.ApplicationScopeData).As<IApplicationData>().ExternallyOwned();
			builder.Register(c => UnitOfWorkFactoryContainer.Current).As<IUnitOfWorkFactory>().ExternallyOwned();
			builder.Register(getThreadJobResultFeedback).As<IJobResultFeedback>().ExternallyOwned();
			builder.RegisterType<SendPushMessageWhenRootAlteredService>().As<ISendPushMessageWhenRootAlteredService>().InstancePerDependency();
			builder.RegisterType<ScenarioProvider>().As<IScenarioProvider>().InstancePerDependency();
			builder.RegisterType<RepositoryFactory>().As<IRepositoryFactory>().InstancePerDependency();
			builder.RegisterType<ScheduleChangedInDefaultScenarioNotification>().As<IScheduleChangedNotification>().InstancePerDependency();
		}

    	private static IJobResultFeedback getThreadJobResultFeedback(IComponentContext componentContext)
    	{
    		return jobResultFeedback ?? (jobResultFeedback = new JobResultFeedback());
    	}
    }
}