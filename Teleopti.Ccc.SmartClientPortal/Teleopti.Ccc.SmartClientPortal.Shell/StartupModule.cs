using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
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
			if (_configuration.Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				builder.RegisterType<CheckMessageBrokerMailBox>().As<ISystemCheck>();
			builder.RegisterType<SystemCheckerValidator>();
			builder.RegisterType<OutlookPanelContentWorker>();

			builder.RegisterType<LogonPresenter>().As<ILogonPresenter>().SingleInstance();

			builder.RegisterType<LogonModel>().SingleInstance();

			builder.RegisterType<LogonLicenseChecker>().As<ILogonLicenseChecker>();
			builder.RegisterType<LicenseVerifierFactory>().As<ILicenseVerifierFactory>();
			builder.RegisterType<LicenseStatusLoader>().As<ILicenseStatusLoader>();
			builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>();
			builder.RegisterType<LoginInitializer>().As<ILoginInitializer>();
			builder.RegisterType<LogonMatrix>().As<ILogonMatrix>();

			if (_configuration.Toggle(Toggles.WfmPermission_ReplaceOldPermission_34671))
			{
				builder.RegisterType<LoginWebView>()
					.As<ILicenseFeedback>()
					.As<ILogonView>()
					.OnActivated(e => e.Instance.Presenter = e.Context.Resolve<ILogonPresenter>())
					.SingleInstance();
			}
			else
			{
				builder.RegisterType<LogonView>()
					 .As<ILicenseFeedback>()
					 .As<ILogonView>()
					 .OnActivated(e => e.Instance.Presenter = e.Context.Resolve<ILogonPresenter>())
					 .SingleInstance();
			}
		}
	}
}
