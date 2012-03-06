using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadFunctionsLightCommand : IExecutableCommand { }

    public class LoadFunctionsLightCommand : ILoadFunctionsLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadFunctionsLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _permissionViewerRoles = permissionViewerRoles;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                var functions = _repositoryFactory.CreateApplicationRolePersonRepository(uow).Functions();
                var list = new List<ListViewItem>();
                foreach (var function in functions)
                {
                    string name = "";
                    if(!string.IsNullOrEmpty(function.ResourceName))
                        name = LanguageResourceHelper.Translate(function.ResourceName);
                    if (string.IsNullOrEmpty(name))
                        name = function.Name;

                    var newFunction = new ListViewItem(name) { Tag = function.Id };
                    newFunction.SubItems.Add(function.Role);

                    list.Add(newFunction);
                }
                _permissionViewerRoles.FillFunctionsMainList(list.ToArray());
            }
        }
    }
}