using System;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.WinCode.Permissions.Commands;
using Teleopti.Ccc.WinCode.Permissions.Events;

namespace Teleopti.Ccc.WinCode.Permissions
{
    public interface IPermissionViewerRolesPresenter
    {
        void ShowViewer();
    }

    public class PermissionViewerRolesPresenter : IPermissionViewerRolesPresenter
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILoadRolesLightCommand _loadRolesCommand;
        private readonly IPermissionViewerRoles _permissionViewerRoles;
        private readonly ILoadPersonsLightCommand _loadPersonsLightCommand;
        private readonly ILoadRolesOnPersonLightCommand _loadRolesOnPersonLightCommand;
        private readonly ILoadFunctionsOnPersonLightCommand _loadFunctionsOnPersonLightCommand;
        private readonly ILoadFunctionsLightCommand _loadFunctionsLightCommand;
        private readonly ILoadPersonsWithFunctionLightCommand _loadPersonsWithFunctionLightCommand;
        private readonly ILoadRolesWithFunctionLightCommand _loadRolesWithFunctionLightCommand;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PermissionViewerRolesPresenter(IEventAggregator eventAggregator, ILoadRolesLightCommand loadRolesCommand, IPermissionViewerRoles permissionViewerRoles, 
            ILoadPersonsLightCommand loadPersonsLightCommand, ILoadRolesOnPersonLightCommand loadRolesOnPersonLightCommand, ILoadFunctionsOnPersonLightCommand loadFunctionsOnPersonLightCommand,
            ILoadFunctionsLightCommand loadFunctionsLightCommand, ILoadPersonsWithFunctionLightCommand loadPersonsWithFunctionLightCommand,
            ILoadRolesWithFunctionLightCommand loadRolesWithFunctionLightCommand)
        {
            _eventAggregator = eventAggregator;
            _loadRolesCommand = loadRolesCommand;
            _permissionViewerRoles = permissionViewerRoles;
            _loadPersonsLightCommand = loadPersonsLightCommand;
            _loadRolesOnPersonLightCommand = loadRolesOnPersonLightCommand;
            _loadFunctionsOnPersonLightCommand = loadFunctionsOnPersonLightCommand;
            _loadFunctionsLightCommand = loadFunctionsLightCommand;
            _loadPersonsWithFunctionLightCommand = loadPersonsWithFunctionLightCommand;
            _loadRolesWithFunctionLightCommand = loadRolesWithFunctionLightCommand;

            _eventAggregator.GetEvent<PersonRolesAndFunctionsNeedLoad>().Subscribe(loadRolePersonsAndFunctions);
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Subscribe(loadFunctionPersonsAndRoles);
        }

        private void loadFunctionPersonsAndRoles(string obj)
        {
            _loadPersonsWithFunctionLightCommand.Execute();
            _loadRolesWithFunctionLightCommand.Execute();
        }

        private void loadRolePersonsAndFunctions(string obj)
        {
            _loadRolesOnPersonLightCommand.Execute();
            _loadFunctionsOnPersonLightCommand.Execute();
        }

        public void ShowViewer()
        {
            _loadPersonsLightCommand.Execute();
            _loadRolesCommand.Execute();
            _loadFunctionsLightCommand.Execute();
            _permissionViewerRoles.Show();
        }
    }
}