using System.Web;
using System.Web.Mvc;
using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

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
			builder.RegisterType<DataSourceProvider>().As<IDataSourceProvider>();
			builder.Register(c => c.Resolve<IDataSourceProvider>().CurrentDataSource())
				.As<IDataSource>()
				.ExternallyOwned();
		}

		private static void registerRequestContextTypes(ContainerBuilder builder)
		{
			builder.RegisterType<LoggedOnUser>().As<ILoggedOnUser>();
			builder.RegisterType<UserTimeZone>().As<IUserTimeZone>();
			builder.RegisterType<SessionPrincipalFactory>().As<ISessionPrincipalFactory>();
			builder.RegisterType<IdentityProvider>().As<IIdentityProvider>();
			builder.RegisterType<CultureProvider>().As<ICultureProvider>();
			builder.RegisterType<RequestContextInitializer>().As<IRequestContextInitializer>();
			builder.RegisterType<SessionSpecificCookieDataProvider>().As<ISessionSpecificDataProvider>();
			builder.RegisterType<DefaultSessionSpecificCookieDataProviderSettings>().As<ISessionSpecificCookieDataProviderSettings>();
			builder.RegisterType<SetThreadCulture>().As<ISetThreadCulture>();
			builder.RegisterType<PermissionProvider>().As<IPermissionProvider>();
			builder.RegisterType<AbsenceTypesProvider>().As<IAbsenceTypesProvider>();
			builder.RegisterType<CurrentBusinessUnitProvider>().As<ICurrentBusinessUnitProvider>();
		}

		private static void registerPortalTypes(ContainerBuilder builder)
		{
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>();
			builder.RegisterType<PortalViewModelFactory>().As<IPortalViewModelFactory>();
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.RegisterType<DatePickerGlobalizationViewModelFactory>().As<IDatePickerGlobalizationViewModelFactory>();
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
			builder.RegisterType<ErrorMessageProvider>().As<IErrorMessageProvider>();
			builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>();
			builder.RegisterType<DefaultScenarioScheduleProvider>()
				.As<IScheduleProvider>()
				.As<IStudentAvailabilityProvider>();
			builder.RegisterType<VirtualSchedulePeriodProvider>().As<IVirtualSchedulePeriodProvider>();
			builder.RegisterType<DefaultDateCalculator>().As<IDefaultDateCalculator>();

			builder.RegisterGeneric(typeof (ResolveUsingDependencyResolver<>)).As(typeof (IResolve<>));
		}
	}

	public interface IResolve<T>
	{
		T Invoke();
	}

	public class ResolveUsingDependencyResolver<T> : IResolve<T>
	{
		private readonly IComponentContext _resolver;

		public ResolveUsingDependencyResolver(IComponentContext resolver) {
			_resolver = resolver;
		}

		public T Invoke()
		{
			if (HttpContext.Current == null)
				return _resolver.Resolve<T>();
			return DependencyResolver.Current.GetService<T>();
		}
	}
}