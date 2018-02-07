﻿using System.Runtime;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	public class StartupModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public StartupModule(IIocConfiguration configuration)
		{
			ProfileOptimization.SetProfileRoot(Application.UserAppDataPath);
			ProfileOptimization.StartProfile("StartupModule.Profile");
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationStartup>().SingleInstance();
			builder.RegisterType<EnvironmentWindowsUserProvider>()
				.As<IWindowsUserProvider>()
				.SingleInstance();
			builder.RegisterType<OutlookPanelContentWorker>();
			builder.Register(c => new WebUrlHolder(_configuration.Args().ReportServer)).SingleInstance();
			builder.RegisterType<LogonPresenter>().SingleInstance();

			builder.RegisterType<LogonModel>().SingleInstance();

			builder.RegisterType<LogonLicenseChecker>().As<ILogonLicenseChecker>();
			builder.RegisterType<LicenseStatusLoader>().As<ILicenseStatusLoader>();
			builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>();
			builder.RegisterType<LoginInitializer>().As<ILoginInitializer>();
			builder.RegisterType<LogonMatrix>().As<ILogonMatrix>();
			builder.RegisterType<LoginWebView>()
				.As<ILicenseFeedback>()
				.As<ILogonView>()
				.OnActivated(e => e.Instance.Presenter = e.Context.Resolve<LogonPresenter>())
				.SingleInstance();
		}
	}
}
