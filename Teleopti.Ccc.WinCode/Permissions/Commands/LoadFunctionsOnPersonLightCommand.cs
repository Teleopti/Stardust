using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Permissions.Commands
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
                ListView list = _permissionViewerRoles.PersonFunctionsList;
                list.Items.Clear();
                foreach (var function in functions)
                {
                    string name = "";
                    if(!string.IsNullOrEmpty(function.ResourceName))
                        name = LanguageResourceHelper.Translate(function.ResourceName);
                    if (string.IsNullOrEmpty(name))
                        name = function.Name;

                    var newFunction = new ListViewItem(name) { Tag = function.Id };
                    newFunction.SubItems.Add(function.Role);

                    list.Items.Add(newFunction);
                }
            }
        }
    }
}