using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces;
using Teleopti.Ccc.Win.Main;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
    public class StartupModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LogOnScreen>()
                .SingleInstance();
            builder.RegisterType<ApplicationStartup>()
                .SingleInstance();
            builder.RegisterType<SmartClientShellForm>().As<SmartClientShellForm>().As<IClientPortalCallback>();
            builder.RegisterType<EnvironmentWindowsUserProvider>()
                .As<IWindowsUserProvider>()
                .SingleInstance();
            builder.RegisterType<CheckMessageBroker>().As<ISystemCheck>();
            builder.Register(c => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging).As
                <IMessageBroker>().ExternallyOwned();
            builder.RegisterType<SystemCheckerValidator>();
            builder.RegisterType<OutlookPanelContentWorker>();
            builder.RegisterType<NewOutlookBarWorkspace>();
            builder.RegisterType<OutlookBarWorkspaceModel>().SingleInstance();
            builder.RegisterType<DelayedDataSourceHandler>().As<IDataSourceHandler>();
        }

        private class DelayedDataSourceHandler : IDataSourceHandler
        {
            private readonly IComponentContext _componentContext;

            public DelayedDataSourceHandler(IComponentContext componentContext)
            {
                _componentContext = componentContext;
            }

            public IAvailableDataSourcesProvider AvailableDataSourcesProvider()
            {
                return _componentContext.Resolve<IAvailableDataSourcesProvider>();
            }

            public IEnumerable<IDataSourceProvider> DataSourceProviders()
            {
                return _componentContext.Resolve<IEnumerable<IDataSourceProvider>>();
            }
        }
    }
}
