using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadRolesOnPersonLightCommand : IExecutableCommand { }

    public class LoadRolesOnPersonLightCommand : ILoadRolesOnPersonLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadRolesOnPersonLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
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
                var roles = _repositoryFactory.CreateApplicationRolePersonRepository(uow).RolesOnPerson(selectedPerson);
                var list = new List<ListViewItem>();
                foreach (var roleLight in roles)
                {
                    //list.Items.Add(roleLight.Id.ToString(), roleLight.Name, 0);
                    list.Add(new ListViewItem(roleLight.Name) {Tag = roleLight.Id});
                }
                _permissionViewerRoles.FillPersonRolesList(list.ToArray());
            }
        }
    }
}