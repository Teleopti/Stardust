using System;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.WinCode.Permissions.Commands;
using Teleopti.Ccc.WinCode.Permissions.Events;

namespace Teleopti.Ccc.WinCode.Permissions
{
    public interface IPermissionViewerRolesPresenter
    {
        void ShowViewer();
        bool Unloaded { get; }
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
        private bool _initialLoadDone;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PermissionViewerRolesPresenter(IEventAggregator eventAggregator, ILoadRolesLightCommand loadRolesCommand, IPermissionViewerRoles permissionViewerRoles, 
            ILoadPersonsLightCommand loadPersonsLightCommand, ILoadRolesOnPersonLightCommand loadRolesOnPersonLightCommand, ILoadFunctionsOnPersonLightCommand loadFunctionsOnPersonLightCommand,
            ILoadFunctionsLightCommand loadFunctionsLightCommand, ILoadPersonsWithFunctionLightCommand loadPersonsWithFunctionLightCommand,
            ILoadRolesWithFunctionLightCommand loadRolesWithFunctionLightCommand)
        {
            _eventAggregator = eventAggregator;
            _initialLoadDone = false;
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

            _eventAggregator.GetEvent<PermissionsViewerUnloaded>().Subscribe(permissionsViewerUnloaded);
        }

        private void permissionsViewerUnloaded(string obj)
        {
            Unloaded = true;
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
            if (!_initialLoadDone)
            {
                _loadPersonsLightCommand.Execute();
                _loadRolesCommand.Execute();
                _loadFunctionsLightCommand.Execute();
                _initialLoadDone = true;
            }
            
            _permissionViewerRoles.Show();
            _permissionViewerRoles.BringToFront();
        }

        public bool Unloaded { get; private set; }
    }
}