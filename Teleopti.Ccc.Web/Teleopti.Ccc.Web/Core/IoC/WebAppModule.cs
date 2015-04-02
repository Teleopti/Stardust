using System.Web.Http;
using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
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
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.IOC;
using Teleopti.Ccc.Web.Areas.SSO.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
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

			builder.RegisterApiControllers(typeof(WebAppModule).Assembly).ApplyAspects();
			builder.RegisterControllers(typeof(WebAppModule).Assembly).ApplyAspects();
			builder.RegisterHubs(typeof(WebAppModule).Assembly).ApplyAspects();

			if (_httpConfiguration != null)
				builder.RegisterWebApiFilterProvider(_httpConfiguration);
			builder.RegisterModule(new AutofacWebTypesModule());
			builder.RegisterFilterProvider();

			builder.RegisterModule(new BootstrapperModule(_configuration));

			builder.RegisterModule(new CommonModule(_configuration));

			builder.RegisterModule<WebModule>();
			builder.RegisterModule(new MyTimeAreaModule(_configuration));
			builder.RegisterModule<SSOAreaModule>();
			builder.RegisterModule<StartAreaModule>();
			builder.RegisterModule(new AnywhereAreaModule(_configuration));
			
			builder.RegisterModule<PerformanceToolAreaModule>();
			builder.RegisterModule<ForecastingAreaModule>();
			builder.RegisterModule<ResourcePlannerModule>();

			builder.RegisterModule(new HangfireModule(_configuration));

			builder.RegisterModule<ResourceHandlerModule>();

			builder.RegisterType<WebRequestPrincipalContext>().As<ICurrentPrincipalContext>().SingleInstance();

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

			builder.RegisterModule(new ConfigurationSettingsReader());
			builder.RegisterModule<TenantModule>();
			builder.RegisterModule<SeatPlannerAreaModule>();
		}
	}
}