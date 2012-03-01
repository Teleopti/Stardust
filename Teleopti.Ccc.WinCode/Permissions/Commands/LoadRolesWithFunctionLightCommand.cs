using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadRolesWithFunctionLightCommand : IExecutableCommand { }

    public class LoadRolesWithFunctionLightCommand : ILoadRolesWithFunctionLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadRolesWithFunctionLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _permissionViewerRoles = permissionViewerRoles;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                Guid selectedFunction = _permissionViewerRoles.SelectedFunction;
                var roles = _repositoryFactory.CreateApplicationRolePersonRepository(uow).RolesWithFunction(selectedFunction);
                ListView list = _permissionViewerRoles.FunctionRolesList;
                list.Items.Clear();
                foreach (var role in roles)
                {
                    list.Items.Add(new ListViewItem(role.Name) { Tag = role.Id });
                }
            }
        }
    }
}