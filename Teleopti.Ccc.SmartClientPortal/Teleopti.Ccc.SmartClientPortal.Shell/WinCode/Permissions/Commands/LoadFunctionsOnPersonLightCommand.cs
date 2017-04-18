using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadFunctionsOnPersonLightCommand : IExecutableCommand { }

    public class LoadFunctionsOnPersonLightCommand : ILoadFunctionsOnPersonLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadFunctionsOnPersonLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _permissionViewerRoles = permissionViewerRoles;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                Guid selectedPerson = _permissionViewerRoles.SelectedPerson;
                var functions = _repositoryFactory.CreateApplicationRolePersonRepository(uow).FunctionsOnPerson(selectedPerson);
                
                checkAvailableFunctions(functions);
            }
        }

        private void checkAvailableFunctions(IEnumerable<IFunctionLight> functionLights)
        {
            var nodes = _permissionViewerRoles.AllFunctionNodes;
            foreach (var treeNodeAdv in nodes)
            {
                treeNodeAdv.Checked = false;
                treeNodeAdv.HelpText = Resources.FromRole;
            }

            foreach (var treeNodeAdv in nodes)
            {
                treeNodeAdv.Checked = false;
                var id = (Guid)treeNodeAdv.TagObject;
                foreach (var functionLight in functionLights.Where(functionLight => functionLight.Id.Equals(id)))
                {
                    treeNodeAdv.Checked = true;
                    treeNodeAdv.HelpText = treeNodeAdv.HelpText + Environment.NewLine +  functionLight.Role;
                }
                
            }
        }
    }
}