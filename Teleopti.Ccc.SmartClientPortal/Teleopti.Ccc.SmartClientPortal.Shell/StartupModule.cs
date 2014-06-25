using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
    public class StartupModule : Module
    {
	    protected override void Load(ContainerBuilder builder)
	    {
		    //builder.RegisterType<LogOnScreen>()
		    //       .SingleInstance();
		    builder.RegisterType<ApplicationStartup>()
		           .SingleInstance();
		    builder.RegisterType<EnvironmentWindowsUserProvider>()
		           .As<IWindowsUserProvider>()
		           .SingleInstance();
		    builder.RegisterType<CheckMessageBroker>().As<ISystemCheck>();
		    builder.Register(c => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging).As
			    <IMessageBroker>().ExternallyOwned();
		    builder.RegisterType<SystemCheckerValidator>();
		    builder.RegisterType<OutlookPanelContentWorker>();

			builder.RegisterType<LogonModel>().SingleInstance();
            builder.RegisterType<LogonPresenter>().As<ILogonPresenter>().SingleInstance();

            builder.RegisterType<LogonLicenseChecker>().As<ILogonLicenseChecker>();
            builder.RegisterType<LicenseVerifierFactory>().As<ILicenseVerifierFactory>();
            builder.RegisterType<LicenseStatusLoader>().As<ILicenseStatusLoader>();
		    builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>();
			builder.RegisterType<LoginInitializer>().As<ILoginInitializer>();
			builder.RegisterType<LogonDataSourceHandler>().As<IDataSourceHandler>();
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
