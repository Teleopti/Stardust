using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadRolesLightCommand : IExecutableCommand { }

    public class LoadRolesLightCommand : ILoadRolesLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadRolesLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _permissionViewerRoles = permissionViewerRoles;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                var roles = _repositoryFactory.CreateApplicationRolePersonRepository(uow).Roles();
                var list = new List<ListViewItem>();
                foreach (var roleLight in roles)
                {
                    //list.Items.Add(roleLight.Id.ToString(), roleLight.Name, 0);
                    list.Add(new ListViewItem(roleLight.Name) {Tag = roleLight.Id});
                }
                _permissionViewerRoles.FillRolesMainList(list.ToArray());
            }
        }
    }
}