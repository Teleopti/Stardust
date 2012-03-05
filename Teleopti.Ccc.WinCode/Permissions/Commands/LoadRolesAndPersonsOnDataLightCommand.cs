using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadRolesAndPersonsOnDataLightCommand : IExecutableCommand { }

    public class LoadRolesAndPersonsOnDataLightCommand : ILoadRolesAndPersonsOnDataLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadRolesAndPersonsOnDataLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _permissionViewerRoles = permissionViewerRoles;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                Guid selectedData = _permissionViewerRoles.SelectedData;
                var roleGuid = new List<Guid>();
                var rep = _repositoryFactory.CreateApplicationRolePersonRepository(uow);
                var roles = rep.RolesWithData((selectedData));
                var list = new List<ListViewItem>();
                foreach (var roleLight in roles)
                {
                    list.Add(new ListViewItem(roleLight.Name) {Tag = roleLight.Id});
                    roleGuid.Add(roleLight.Id);
                }

                _permissionViewerRoles.FillDataRolesList(list.ToArray());
                IList<IPersonInRole> persons = new List<IPersonInRole>();
                if(roleGuid.Count > 0)
                    persons = rep.PersonsWithRoles(roleGuid);

                var list2 = new List<ListViewItem>();
                foreach (var person in persons)
                {
                    var newPerson = new ListViewItem(person.FirstName) { Tag = person.Id };
                    newPerson.SubItems.Add(person.LastName);
                    newPerson.SubItems.Add(person.Team);
                    list2.Add(newPerson);
                }
                _permissionViewerRoles.FillDataPersonsList(list2.ToArray());
            }
        }
    }
}