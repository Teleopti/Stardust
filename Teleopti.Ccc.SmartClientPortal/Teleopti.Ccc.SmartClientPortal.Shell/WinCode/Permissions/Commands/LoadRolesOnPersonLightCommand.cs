using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands
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
                    var name = LanguageResourceHelper.Translate(roleLight.Name);
                    if (string.IsNullOrEmpty(name)) name = roleLight.Name;
                    list.Add(new ListViewItem(name) {Tag = roleLight.Id});
                }
                _permissionViewerRoles.FillPersonRolesList(list.ToArray());
            }
        }
    }
}