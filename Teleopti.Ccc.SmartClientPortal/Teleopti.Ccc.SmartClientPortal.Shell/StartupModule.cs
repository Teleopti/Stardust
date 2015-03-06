using Autofac;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	public class StartupModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public StartupModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationStartup>()
					 .SingleInstance();
			builder.RegisterType<EnvironmentWindowsUserProvider>()
					 .As<IWindowsUserProvider>()
					 .SingleInstance();
			builder.RegisterType<CheckMessageBroker>().As<ISystemCheck>();
			builder.RegisterType<SystemCheckerValidator>();
			builder.RegisterType<OutlookPanelContentWorker>();

			builder.RegisterType<MultiTenancyApplicationLogon>().As<IMultiTenancyApplicationLogon>().SingleInstance();
			builder.RegisterType<MultiTenancyWindowsLogon>().As<IMultiTenancyWindowsLogon>().SingleInstance();
			builder.RegisterType<MultiTenancyLogonPresenter>().As<ILogonPresenter>().SingleInstance();

			builder.RegisterType<LogonModel>().SingleInstance();

			builder.RegisterType<LogonLicenseChecker>().As<ILogonLicenseChecker>();
			builder.RegisterType<LicenseVerifierFactory>().As<ILicenseVerifierFactory>();
			builder.RegisterType<LicenseStatusLoader>().As<ILicenseStatusLoader>();
			builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>();
			builder.RegisterType<LoginInitializer>().As<ILoginInitializer>();
			//builder.RegisterType<LogonDataSourceHandler>().As<IDataSourceHandler>();
			builder.RegisterType<ServerEndpointSelector>().As<IServerEndpointSelector>();
			builder.RegisterType<LogonMatrix>().As<ILogonMatrix>();

			builder.RegisterType<LogonView>()
					 .As<ILicenseFeedback>()
					 .As<ILogonView>()
					 .OnActivated(e => e.Instance.Presenter = e.Context.Resolve<ILogonPresenter>())
					 .SingleInstance();
		}
	}
}
