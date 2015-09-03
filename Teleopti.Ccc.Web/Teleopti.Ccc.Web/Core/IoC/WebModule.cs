using System.Web.Http;
using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC;
using Teleopti.Ccc.Web.Areas.Mart.Core.IoC;
using Teleopti.Ccc.Web.Areas.Messages.Core.Ioc;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.IoC;
using Teleopti.Ccc.Web.Areas.People.Core.IoC;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Core.IoC;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.IOC;
using Teleopti.Ccc.Web.Areas.SSO.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Hangfire;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class WebModule : Module
	{
		private readonly IIocConfiguration _configuration;
		private readonly HttpConfiguration _httpConfiguration;

		public WebModule(IIocConfiguration configuration, HttpConfiguration httpConfiguration)
		{
			_configuration = configuration;
			_httpConfiguration = httpConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterApiControllers(typeof(WebModule).Assembly).ApplyAspects();
			builder.RegisterControllers(typeof(WebModule).Assembly).ApplyAspects();
			builder.RegisterHubs(typeof(WebModule).Assembly).ApplyAspects();

			builder.RegisterModule<MessageBrokerWebModule>();

			if (_httpConfiguration != null)
				builder.RegisterWebApiFilterProvider(_httpConfiguration);
			builder.RegisterModule(new AutofacWebTypesModule());
			builder.RegisterFilterProvider();

			builder.RegisterModule(new BootstrapperModule(_configuration));

			registerCommonTypes(builder);
			registerPortalTypes(builder);
			registerRequestContextTypes(builder);

			builder.RegisterModule(new MyTimeAreaModule(_configuration));
			builder.RegisterModule<SSOAreaModule>();
			builder.RegisterModule<StartAreaModule>();
			builder.RegisterModule(new AnywhereAreaModule(_configuration));

			builder.RegisterModule<PerformanceToolAreaModule>();
			builder.RegisterModule<ForecastingAreaModule>();
			builder.RegisterModule(new ResourcePlannerModule());

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
		
			builder.RegisterType<NumberOfAgentsInSiteReader>().As<INumberOfAgentsInSiteReader>().SingleInstance();
			builder.RegisterType<NumberOfAgentsInTeamReader>().As<INumberOfAgentsInTeamReader>().SingleInstance();
			builder.RegisterType<SiteAdherenceAggregator>().As<ISiteAdherenceAggregator>().SingleInstance();
			builder.RegisterType<TeamAdherenceAggregator>().As<ITeamAdherenceAggregator>().SingleInstance();

			builder.RegisterModule(new ConfigurationSettingsReader());
			builder.RegisterModule(new TenantServerModule(_configuration));
			builder.RegisterModule<SeatPlannerAreaModule>();
			builder.RegisterModule<OutboundAreaModule>();
			builder.RegisterModule<PeopleAreaModule>();

			tenantWebSpecificTypes(builder);
		}

		private static void registerRequestContextTypes(ContainerBuilder builder)
		{
			builder.RegisterType<SessionPrincipalFactory>().As<ISessionPrincipalFactory>();
			builder.RegisterType<RequestContextInitializer>().As<IRequestContextInitializer>();
			builder.RegisterType<SessionSpecificCookieDataProvider>().As<ISessionSpecificDataProvider>();
			builder.RegisterType<SessionSpecificForIdentityProviderDataProvider>().As<ISessionSpecificForIdentityProviderDataProvider>();
			builder.RegisterType<SessionAuthenticationModule>().As<ISessionAuthenticationModule>();
			builder.RegisterType<DefaultSessionSpecificCookieDataProviderSettings>().As<ISessionSpecificCookieDataProviderSettings>();
			builder.RegisterType<DefaultSessionSpecificCookieForIdentityProviderDataProviderSettings>().As<ISessionSpecificCookieForIdentityProviderDataProviderSettings>();
			builder.RegisterType<SetThreadCulture>().As<ISetThreadCulture>();
			builder.RegisterType<PermissionProvider>().As<IPermissionProvider>();
			builder.RegisterType<AbsenceTypesProvider>().As<IAbsenceTypesProvider>();
			builder.RegisterType<PushMessageProvider>().As<IPushMessageProvider>();
			builder.RegisterType<ReportsProvider>().As<IReportsProvider>();
			builder.RegisterType<ReportsNavigationProvider>().As<IReportsNavigationProvider>();
			builder.RegisterType<BadgeProvider>().As<IBadgeProvider>();
			builder.RegisterType<HttpRequestTrue>().As<IIsHttpRequest>().SingleInstance();
		}

		private static void registerPortalTypes(ContainerBuilder builder)
		{
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
		}

		private static void registerCommonTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ResourceVersion>().As<IResourceVersion>();
			builder.RegisterType<ErrorMessageProvider>().As<IErrorMessageProvider>();
			builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>();
			builder.RegisterType<DefaultScenarioScheduleProvider>().As<IScheduleProvider>();
			builder.RegisterType<DefaultScenarioForStudentAvailabilityScheduleProvider>()
				.As<IStudentAvailabilityProvider>();
			builder.RegisterType<VirtualSchedulePeriodProvider>().As<IVirtualSchedulePeriodProvider>();
			builder.RegisterType<DefaultDateCalculator>().As<IDefaultDateCalculator>();
			builder.RegisterType<UrlHelperProvider>().As<IUrlHelper>().SingleInstance();
			builder.RegisterType<IpAddressResolver>().As<IIpAddressResolver>();
			builder.RegisterType<AuthenticationModule>().As<IAuthenticationModule>().SingleInstance();
			builder.RegisterType<IdentityProviderProvider>().As<IIdentityProviderProvider>().SingleInstance();
            builder.RegisterType<IanaTimeZoneProvider>().As<IIanaTimeZoneProvider>().SingleInstance();
		}

		private static void tenantWebSpecificTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<IdentityAuthentication>().As<IIdentityAuthentication>().SingleInstance();
			builder.RegisterType<DataSourceConfigurationEncryption>().As<IDataSourceConfigurationEncryption>().SingleInstance();
			builder.RegisterType<PersonInfoMapper>().As<IPersonInfoMapper>().SingleInstance();
			builder.RegisterType<ChangePersonPassword>().As<IChangePersonPassword>().SingleInstance();
			builder.RegisterType<WebTenantAuthentication>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<CurrentTenantUser>().As<ICurrentTenantUser>().SingleInstance();
		}
	}
}