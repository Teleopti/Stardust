using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class CommonModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			registerCommonTypes(builder);
			registerPortalTypes(builder);
			registerRequestContextTypes(builder);
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
			builder.RegisterType<CurrentBusinessUnit>().As<ICurrentBusinessUnit>();
			builder.RegisterType<PushMessageProvider>().As<IPushMessageProvider>();
			builder.RegisterType<ReportsProvider>().As<IReportsProvider>();
			builder.RegisterType<ReportsNavigationProvider>().As<IReportsNavigationProvider>();
			builder.RegisterType<BadgeProvider>().As<IBadgeProvider>();
			builder.RegisterType<TeamScheduleBadgeProvider>().As<ITeamScheduleBadgeProvider>();
			builder.RegisterType<BadgeSettingProvider>().As<IBadgeSettingProvider>();
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
			builder.RegisterType<DefaultScenarioScheduleProvider>()
				.As<IScheduleProvider>()
				.As<IStudentAvailabilityProvider>();
			builder.RegisterType<VirtualSchedulePeriodProvider>().As<IVirtualSchedulePeriodProvider>();
			builder.RegisterType<DefaultDateCalculator>().As<IDefaultDateCalculator>();
			builder.RegisterType<UrlHelperProvider>().As<IUrlHelper>().SingleInstance();
			builder.Register(c => SignalRConfiguration.ActionScheduler).As<IActionScheduler>();
			builder.RegisterType<SubscriptionFiller>().As<IBeforeSubscribe>();
			builder.RegisterType<IpAddressResolver>().As<IIpAddressResolver>();
			builder.RegisterType<AuthenticationModule>().As<IAuthenticationModule>().SingleInstance();
			builder.RegisterType<IdentityProviderProvider>().As<IIdentityProviderProvider>().SingleInstance();
            builder.RegisterType<IanaTimeZoneProvider>().As<IIanaTimeZoneProvider>().SingleInstance();
		}
	}
}