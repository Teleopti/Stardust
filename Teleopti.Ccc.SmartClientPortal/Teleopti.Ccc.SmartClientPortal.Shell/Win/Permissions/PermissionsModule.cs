using Autofac;
using Teleopti.Ccc.WinCode.Permissions;
using Teleopti.Ccc.WinCode.Permissions.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Permissions
{
    public class PermissionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            overviewMain(builder);
        }
        
        private static void overviewMain(ContainerBuilder builder)
        {
            builder.RegisterType<LoadPersonsLightCommand>().As<ILoadPersonsLightCommand>().InstancePerLifetimeScope();
            builder.RegisterType<PermissionViewer>().As<IPermissionViewerRoles>().InstancePerLifetimeScope();
            builder.RegisterType<PermissionViewerRolesPresenter>().As<IPermissionViewerRolesPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<PermissionsExplorerHelper>().InstancePerLifetimeScope();
            builder.RegisterType<LoadRolesOnPersonLightCommand>().As<ILoadRolesOnPersonLightCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadFunctionsOnPersonLightCommand>().As<ILoadFunctionsOnPersonLightCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadFunctionsLightCommand>().As<ILoadFunctionsLightCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadPersonsWithFunctionLightCommand>().As<ILoadPersonsWithFunctionLightCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadRolesWithFunctionLightCommand>().As<ILoadRolesWithFunctionLightCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadDataOnPersonsLightCommand>().As<ILoadDataOnPersonsLightCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadRolesAndPersonsOnDataLightCommand>().As<ILoadRolesAndPersonsOnDataLightCommand>().InstancePerLifetimeScope();
            builder.RegisterType<LoadRolesAndPersonsOnDataRangeLightCommand>().As<ILoadRolesAndPersonsOnDataRangeLightCommand>().InstancePerLifetimeScope();
            
        }  
    }
}