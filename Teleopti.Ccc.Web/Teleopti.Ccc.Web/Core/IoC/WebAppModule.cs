using System.Configuration;
using System.Web.Http;
using Autofac;
using Autofac.Configuration;
using Autofac.Extras.DynamicProxy2;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC;
using Teleopti.Ccc.Web.Areas.Mart.Core.IoC;
using Teleopti.Ccc.Web.Areas.Messages.Core.Ioc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.IoC;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Core.IoC;
using Teleopti.Ccc.Web.Areas.Rta.Core.IoC;
using Teleopti.Ccc.Web.Areas.SSO.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Hangfire;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class WebAppModule : Module
	{
		private readonly IIocConfiguration _configuration;
		private readonly HttpConfiguration _httpConfiguration;

		public WebAppModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		public WebAppModule(IIocConfiguration configuration, HttpConfiguration httpConfiguration)
		{
			_configuration = configuration;
			_httpConfiguration = httpConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			builder.RegisterApiControllers(typeof(WebAppModule).Assembly).EnableClassInterceptors().InterceptedBy(typeof(AspectInterceptor));
			builder.RegisterControllers(typeof(WebAppModule).Assembly).EnableClassInterceptors().InterceptedBy(typeof(AspectInterceptor));
			builder.RegisterHubs(typeof(WebAppModule).Assembly).EnableClassInterceptors();

			if (_httpConfiguration != null)
				builder.RegisterWebApiFilterProvider(_httpConfiguration);
			builder.RegisterModule(new AutofacWebTypesModule());
			builder.RegisterFilterProvider();

			builder.RegisterModule(new BootstrapperModule(_configuration));

			builder.RegisterModule(new CommonModule(_configuration));

			builder.RegisterModule<WebModule>();
			builder.RegisterModule<MyTimeAreaModule>();
			builder.RegisterModule<SSOAreaModule>();
			builder.RegisterModule<StartAreaModule>();
			builder.RegisterModule<AnywhereAreaModule>();
			builder.RegisterModule<PerformanceToolAreaModule>();
			builder.RegisterModule<ForecastingAreaModule>();

			builder.RegisterModule(new HangfireModule(_configuration));

			builder.RegisterModule<ResourceHandlerModule>();

			builder.RegisterType<WebRequestPrincipalContext>().As<ICurrentPrincipalContext>().SingleInstance();

			builder.RegisterType<UnitOfWorkAspect>();

			builder.RegisterModule(new RuleSetModule(_configuration, false));
			builder.RegisterModule(new AuthenticationCachedModule(_configuration));

			builder.RegisterModule(new RtaAreaModule(_configuration));
			builder.RegisterModule(new MartAreaModule(_configuration));

			builder.RegisterModule<ShiftTradeModule>();
			builder.RegisterModule<NotificationModule>();

			builder.RegisterModule<CommandDispatcherModule>();
			builder.RegisterModule<CommandHandlersModule>();
			builder.RegisterType<EventsMessageSender>().As<IMessageSender>().SingleInstance();

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
		}
	}
}