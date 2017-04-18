using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadPersonsWithFunctionLightCommand : IExecutableCommand { }

    public class LoadPersonsWithFunctionLightCommand : ILoadPersonsWithFunctionLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadPersonsWithFunctionLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
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
                var persons = _repositoryFactory.CreateApplicationRolePersonRepository(uow).PersonsWithFunction(selectedFunction);
                var list = new List<ListViewItem>();
                foreach (var person in persons)
                {
                    var newPerson = new ListViewItem(person.FirstName) { Tag = person.Id };
                    newPerson.SubItems.Add(person.LastName);
                    newPerson.SubItems.Add(person.Team);
                    list.Add(newPerson);
                }
                _permissionViewerRoles.FillFunctionPersonsList(list.ToArray());
            }
        }
    }
}