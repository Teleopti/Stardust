using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Interfaces.Domain;
using IDataSourceProvider = Teleopti.Ccc.Web.Core.RequestContext.IDataSourceProvider;

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
			builder.RegisterType<WebReportRepository>().As<IWebReportRepository>().SingleInstance();
			builder.RegisterType<DataSourceProvider>().As<IDataSourceProvider>().SingleInstance();
			builder.Register(c => c.Resolve<IDataSourceProvider>().CurrentDataSource())
				.As<IDataSource>()
				.ExternallyOwned();
		}

		private static void registerRequestContextTypes(ContainerBuilder builder)
		{
			builder.RegisterType<SessionPrincipalFactory>().As<ISessionPrincipalFactory>().SingleInstance();
			builder.RegisterType<IdentityProvider>().As<IIdentityProvider>().SingleInstance();
			builder.RegisterType<RequestContextInitializer>().As<IRequestContextInitializer>().SingleInstance();
			builder.RegisterType<SessionSpecificCookieDataProvider>().As<ISessionSpecificDataProvider>().SingleInstance();
			builder.RegisterType<DefaultSessionSpecificCookieDataProviderSettings>().As<ISessionSpecificCookieDataProviderSettings>().SingleInstance();
			builder.RegisterType<SetThreadCulture>().As<ISetThreadCulture>().SingleInstance();
			builder.RegisterType<PermissionProvider>().As<IPermissionProvider>().SingleInstance();
			builder.RegisterType<AbsenceTypesProvider>().As<IAbsenceTypesProvider>().SingleInstance();
			builder.RegisterType<CurrentBusinessUnitProvider>().As<ICurrentBusinessUnitProvider>().SingleInstance();
			builder.RegisterType<PushMessageProvider>().As<IPushMessageProvider>().SingleInstance();
		}

		private static void registerPortalTypes(ContainerBuilder builder)
		{
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>().SingleInstance();
			builder.RegisterType<PortalViewModelFactory>().As<IPortalViewModelFactory>().SingleInstance();
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>().SingleInstance();
			builder.RegisterType<DatePickerGlobalizationViewModelFactory>().As<IDatePickerGlobalizationViewModelFactory>().SingleInstance();
			builder.Register(c =>
			                 	{
			                 		if (DefinedLicenseDataFactory.LicenseActivator == null)
			                 			throw new DataSourceException("Missing datasource (no *.hbm.xml file available)!");
			                 		return DefinedLicenseDataFactory.LicenseActivator;
			                 	})
				.As<ILicenseActivator>().SingleInstance();
		}

		private static void registerCommonTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ErrorMessageProvider>().As<IErrorMessageProvider>().SingleInstance();
			builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>().SingleInstance();
			builder.RegisterType<DefaultScenarioScheduleProvider>()
				.As<IScheduleProvider>()
				.As<IStudentAvailabilityProvider>()
				.SingleInstance();
			builder.RegisterType<VirtualSchedulePeriodProvider>().As<IVirtualSchedulePeriodProvider>().SingleInstance();
			builder.RegisterType<DefaultDateCalculator>().As<IDefaultDateCalculator>().SingleInstance();
		}
	}
}