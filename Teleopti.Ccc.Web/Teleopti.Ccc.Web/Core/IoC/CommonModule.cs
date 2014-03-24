using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
			registerReportTypes(builder);
		}

		private static void registerReportTypes(ContainerBuilder builder)
		{
			builder.RegisterType<WebReportRepository>().As<IWebReportRepository>();
		}

		private static void registerRequestContextTypes(ContainerBuilder builder)
		{
			builder.RegisterType<SessionPrincipalFactory>().As<ISessionPrincipalFactory>();
			builder.RegisterType<RequestContextInitializer>().As<IRequestContextInitializer>();
			builder.RegisterType<SessionSpecificCookieDataProvider>().As<ISessionSpecificDataProvider>();
			builder.RegisterType<DefaultSessionSpecificCookieDataProviderSettings>().As<ISessionSpecificCookieDataProviderSettings>();
			builder.RegisterType<SetThreadCulture>().As<ISetThreadCulture>();
			builder.RegisterType<PermissionProvider>().As<IPermissionProvider>();
			builder.RegisterType<AbsenceTypesProvider>().As<IAbsenceTypesProvider>();
			builder.RegisterType<CurrentBusinessUnit>().As<ICurrentBusinessUnit>();
			builder.RegisterType<PushMessageProvider>().As<IPushMessageProvider>();
		}

		private static void registerPortalTypes(ContainerBuilder builder)
		{
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.Register(c =>
			                 	{
			                 		if (DefinedLicenseDataFactory.LicenseActivator == null)
			                 			throw new DataSourceException("Missing datasource (no *.hbm.xml file available)!");
			                 		return DefinedLicenseDataFactory.LicenseActivator;
			                 	})
				.As<ILicenseActivator>();
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
		}
	}
}