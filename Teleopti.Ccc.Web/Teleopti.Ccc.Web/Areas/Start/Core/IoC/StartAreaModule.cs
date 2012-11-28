﻿using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.Start.Core.IoC
{
	public class StartAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<WebLogOn>().As<IWebLogOn>();
			builder.RegisterType<DataSourcesProvider>().As<IDataSourcesProvider>().SingleInstance();
			builder.RegisterType<Authenticator>().As<IAuthenticator>().SingleInstance();
			builder.RegisterType<BusinessUnitProvider>().As<IBusinessUnitProvider>();
			builder.RegisterType<WindowsAccountProvider>().As<IWindowsAccountProvider>().SingleInstance();
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>();
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.RegisterType<AuthenticationViewModelFactory>().As<IAuthenticationViewModelFactory>();
			builder.RegisterType<FormsAuthenticationWrapper>().As<IFormsAuthentication>();
			builder.RegisterType<AvailableWindowsDataSources>().As<IAvailableWindowsDataSources>().SingleInstance();
			builder.RegisterType<MenuViewModelFactory>().As<IMenuViewModelFactory>();
			builder.RegisterType<SessionSpecificDataStringSerializer>().As<ISessionSpecificDataStringSerializer>().SingleInstance();

			builder.RegisterType<BusinessUnitsViewModelFactory>().As<IBusinessUnitsViewModelFactory>();
			builder.RegisterAssemblyTypes(GetType().Assembly)
				.AssignableTo<IAuthenticationType>()
				.As<IAuthenticationType>()
				.SingleInstance();
			builder.RegisterType<DataSourcesViewModelFactory>().As<IDataSourcesViewModelFactory>().SingleInstance();
		}
	}
}
