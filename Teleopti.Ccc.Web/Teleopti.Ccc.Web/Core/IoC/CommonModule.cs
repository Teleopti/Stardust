using System;
using System.Drawing;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
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
			registerAsmTypes(builder);
		}

		private static void registerAsmTypes(ContainerBuilder builder)
		{
			builder.RegisterType<TempAsmModelFactory>().As<IAsmViewModelFactory>();
		}

		public class TempAsmModelFactory : IAsmViewModelFactory
		{
			public AsmViewModel CreateViewModel()
			{
				var model = new AsmViewModel {StartDate = new DateTime(2012, 8,22)};
				model.Layers.Add(new AsmLayer
				                 	{
				                 		Payload = "phone",
				                 		RelativeStartInMinutes = 1600,
				                 		LengthInMinutes = 120,
				                 		Color = ColorTranslator.ToHtml(Color.Red)
				                 	});
				model.Layers.Add(new AsmLayer
				                 	{
				                 		Payload = "pruttibangng",
				                 		RelativeStartInMinutes = 1700,
				                 		LengthInMinutes = 150,
				                 		Color = ColorTranslator.ToHtml(Color.Blue)
				                 	});
				return model;
			}
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
			T result = HttpContext.Current == null ? 
				_resolver.Resolve<T>() : 
				DependencyResolver.Current.GetService<T>();
			if (result == null)
				throw new Exception("Failed to resolve " + typeof(T).Name);
			return result;
		}
	}
}