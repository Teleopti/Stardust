﻿using System.Configuration;
using System.Dynamic;
using System.Reflection;
using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Mvc;
using MbCache.Configuration;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.IoC;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Core.IoC;
using Teleopti.Ccc.Web.Areas.SSO.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ContainerConfiguration : IContainerConfiguration
	{

		public IContainer Configure()
		{
			var builder = new ContainerBuilder();

			builder.RegisterControllers(Assembly.GetExecutingAssembly());

			builder.RegisterModule(new AutofacWebTypesModule());
			builder.RegisterType<CurrentHttpContext>().As<ICurrentHttpContext>().SingleInstance();

			builder.RegisterFilterProvider();

			builder.RegisterModule<BootstrapperModule>();

			builder.RegisterModule<CommonModule>();
			builder.RegisterModule<MyTimeAreaModule>();
			builder.RegisterModule<SSOAreaModule>();
			builder.RegisterModule<StartAreaModule>();
			builder.RegisterModule<MobileReportsAreaModule>();
			builder.RegisterModule<AnywhereAreaModule>();
			builder.RegisterModule<PerformanceToolAreaModule>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule(new InitializeModule(DataSourceConfigurationSetter.ForWeb()));
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<LogModule>();

			builder.RegisterModule<AuthenticationModule>();
			builder.RegisterType<WebRequestPrincipalContext>().As<ICurrentPrincipalContext>().SingleInstance();

			registerAopComponents(builder);

			var mbCacheModule = new MbCacheModule(new InMemoryCache(20), new FixedNumberOfLockObjects(100));
			builder.RegisterModule(mbCacheModule);
			builder.RegisterModule(new RuleSetModule(mbCacheModule, false));
			builder.RegisterModule(new AuthenticationCachedModule(mbCacheModule));

			builder.RegisterModule<ShiftTradeModule>();

			builder.RegisterModule<CommandDispatcherModule>();
			//builder.RegisterModule<LocalInMemoryEventsPublisherModule>();
			builder.RegisterModule<ServiceBusEventsPublisherModule>();
			builder.RegisterModule<CommandHandlersModule>();
			builder.RegisterModule<EventHandlersModule>();
			builder.RegisterType<EventsMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<DoNotNotifySmsLink>().As<IDoNotifySmsLink>().SingleInstance();
			builder.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>().SingleInstance();
			builder.RegisterType<NewtonsoftJsonDeserializer>().As<IJsonDeserializer>().SingleInstance();

			// ErikS: Bug 25359
			var useNewResourceCalculationConfiguration = ConfigurationManager.AppSettings["EnableNewResourceCalculation"];
			if (useNewResourceCalculationConfiguration != null && bool.Parse(useNewResourceCalculationConfiguration))
			{
				builder.RegisterType<ScheduledResourcesReadModelStorage>()
					   .As<IScheduledResourcesReadModelPersister>()
					   .As<IScheduledResourcesReadModelReader>()
					   .SingleInstance();
				builder.RegisterType<ScheduledResourcesReadModelUpdater>()
					.As<IScheduledResourcesReadModelUpdater>().SingleInstance();
			}
			else
			{
				builder.RegisterType<DisabledScheduledResourcesReadModelStorage>()
					   .As<IScheduledResourcesReadModelPersister>()
					   .As<IScheduledResourcesReadModelReader>()
					   .SingleInstance();
				builder.RegisterType<DisabledScheduledResourcesReadModelUpdater>()
					.As<IScheduledResourcesReadModelUpdater>().SingleInstance();
			}


			builder.RegisterModule(new ConfigurationSettingsReader());

			return builder.Build();
		}
	
		private static void registerAopComponents(ContainerBuilder builder)
		{
			builder.RegisterModule<AspectsModule>();
			builder.RegisterType<UnitOfWorkAspect>();
		}

	}
}
