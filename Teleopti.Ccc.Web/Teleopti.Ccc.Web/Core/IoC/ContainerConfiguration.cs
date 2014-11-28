using System.Configuration;
using System.Web.Http;
using Autofac;
using Autofac.Configuration;
using Autofac.Extras.DynamicProxy2;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using MbCache.Configuration;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC;
using Teleopti.Ccc.Web.Areas.Mart.Core.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.IoC;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Core.IoC;
using Teleopti.Ccc.Web.Areas.Rta.Core.IoC;
using Teleopti.Ccc.Web.Areas.SSO.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ContainerConfiguration : IContainerConfiguration
	{
		public IContainer Configure(string featureTogglePath, HttpConfiguration httpConfiguration)
		{
			var builder = new ContainerBuilder();

			builder.RegisterApiControllers(typeof(ContainerConfiguration).Assembly).EnableClassInterceptors().InterceptedBy(typeof(AspectInterceptor));
			builder.RegisterControllers(typeof(ContainerConfiguration).Assembly).EnableClassInterceptors().InterceptedBy(typeof(AspectInterceptor));
			builder.RegisterHubs(typeof(ContainerConfiguration).Assembly).EnableClassInterceptors();

			builder.RegisterWebApiFilterProvider(httpConfiguration);
			builder.RegisterModule(new AutofacWebTypesModule());
			builder.RegisterFilterProvider();

			builder.RegisterModule<BootstrapperModule>();

			var args = new IocArgs
			{
				FeatureToggle = featureTogglePath,
				CacheLockObjectGenerator = new FixedNumberOfLockObjects(100),
				DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForWeb()
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(new IocConfiguration(args, null)));
			builder.RegisterModule(new CommonModule(configuration));

			builder.RegisterModule<WebModule>();
			builder.RegisterModule<ResourceHandlerModule>();
			builder.RegisterModule<MyTimeAreaModule>();
			builder.RegisterModule<SSOAreaModule>();
			builder.RegisterModule<StartAreaModule>();
			builder.RegisterModule<AnywhereAreaModule>();
			builder.RegisterModule<PerformanceToolAreaModule>();
			builder.RegisterModule<ForecastingAreaModule>();

			builder.RegisterType<WebRequestPrincipalContext>().As<ICurrentPrincipalContext>().SingleInstance();

			builder.RegisterType<UnitOfWorkAspect>();

			builder.RegisterModule(new RuleSetModule(configuration, false));
			builder.RegisterModule(new AuthenticationCachedModule(configuration));

			builder.RegisterModule(new RtaAreaModule(configuration));
			builder.RegisterModule(new MartAreaModule(configuration));

			builder.RegisterModule<ShiftTradeModule>();

			builder.RegisterModule<CommandDispatcherModule>();
			//builder.RegisterModule<LocalInMemoryEventsPublisherModule>();
			builder.RegisterModule<ServiceBusEventsPublisherModule>();
			builder.RegisterModule<CommandHandlersModule>();
			builder.RegisterType<EventsMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<DoNotUseSmsLink>().As<INotificationValidationCheck>().SingleInstance();

			builder.RegisterType<NumberOfAgentsInSiteReader>().As<INumberOfAgentsInSiteReader>().SingleInstance();
			builder.RegisterType<NumberOfAgentsInTeamReader>().As<INumberOfAgentsInTeamReader>().SingleInstance();
			builder.RegisterType<SiteAdherenceAggregator>().As<ISiteAdherenceAggregator>().SingleInstance();
			builder.RegisterType<TeamAdherenceAggregator>().As<ITeamAdherenceAggregator>().SingleInstance();
			builder.RegisterType<AgentStatesReader>().As<IAgentStateReader>().SingleInstance();
			
			// ErikS: Bug 25359
			if (ConfigurationManager.AppSettings.GetBoolSetting("EnableNewResourceCalculation"))
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
	}
}
