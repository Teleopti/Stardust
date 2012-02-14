using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;

namespace Teleopti.Ccc.Web.Areas.Start.Core.IoC
{
	public class AuthenticationAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<WebLogOn>().As<IWebLogOn>();
			builder.RegisterType<DataSourcesProvider>().As<IDataSourcesProvider>();
			builder.RegisterType<Authenticator>().As<IAuthenticator>();
			builder.RegisterType<BusinessUnitProvider>().As<IBusinessUnitProvider>();
			builder.RegisterType<WindowsAccountProvider>().As<IWindowsAccountProvider>();
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>();
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.RegisterType<AuthenticationViewModelFactory>().As<IAuthenticationViewModelFactory>();
			builder.RegisterType<FormsAuthenticationWrapper>().As<IFormsAuthentication>();
			builder.RegisterType<AvailableWindowsDataSources>().As<IAvailableWindowsDataSources>();
			builder.RegisterType<Redirector>().As<IRedirector>();
		}
	}
}
