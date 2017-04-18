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
                var list = new List<ListViewItem>();
                foreach (var role in roles)
                {
                    var name = LanguageResourceHelper.Translate(role.Name);
                    if (string.IsNullOrEmpty(name)) name = role.Name;
                    list.Add(new ListViewItem(name) { Tag = role.Id });
                }
                _permissionViewerRoles.FillFunctionRolesList(list.ToArray());
            }
        }
    }
}