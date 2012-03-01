using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Permissions.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadPersonsLightCommand : IExecutableCommand { }

    public class LoadPersonsLightCommand : ILoadPersonsLightCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPermissionViewerRoles _permissionViewerRoles;

        public LoadPersonsLightCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IPermissionViewerRoles permissionViewerRoles)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _permissionViewerRoles = permissionViewerRoles;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                var persons = _repositoryFactory.CreateApplicationRolePersonRepository(uow).Persons();
                ListView list = _permissionViewerRoles.PersonsMainList;
                list.Items.Clear();
                foreach (var person in persons)
                {
                    var newPerson = new ListViewItem(person.FirstName) {Tag = person.Id};
                    newPerson.SubItems.Add(person.LastName);
                    newPerson.SubItems.Add(person.Team);
                    list.Items.Add(newPerson);
                }
            }
        }
    }
}