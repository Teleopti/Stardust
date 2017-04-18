using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Permissions.Commands
{
    [TestFixture]
    public class LoadRolesAndPersonsOnDataLightCommandTest
    {
        private MockRepository _mocks;
        private  IUnitOfWorkFactory _unitOfWorkFactory;
        private  IRepositoryFactory _repositoryFactory;
        private  IPermissionViewerRoles _permissionViewerRoles;
        private LoadRolesAndPersonsOnDataLightCommand _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _permissionViewerRoles = _mocks.StrictMock<IPermissionViewerRoles>();
            _target = new LoadRolesAndPersonsOnDataLightCommand(_unitOfWorkFactory, _repositoryFactory, _permissionViewerRoles);
        }

        [Test]
        public void ShouldGetPersonsFromRepAndLoadListView()
        {
            var id = Guid.NewGuid();
            
            var roles = new List<IRoleLight> {new RoleLight {Id = Guid.NewGuid(), Name = "rolle"}};
            var uow = _mocks.StrictMock<IStatelessUnitOfWork>();
            var rep = _mocks.StrictMock<IApplicationRolePersonRepository>();
            Expect.Call(_permissionViewerRoles.SelectedData).Return(id);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateApplicationRolePersonRepository(uow)).Return(rep);
            Expect.Call(rep.RolesWithData(id)).Return(roles);
            Expect.Call(rep.PersonsWithRoles(new List<Guid>())).Return(new List<IPersonInRole> { new PersonInRole() { Id = Guid.NewGuid(), FirstName = "Admin", LastName = "istrator" } }).IgnoreArguments();
            Expect.Call(() =>_permissionViewerRoles.FillDataPersonsList(new ListViewItem[0])).IgnoreArguments();
            Expect.Call(() => _permissionViewerRoles.FillDataRolesList(new ListViewItem[0])).IgnoreArguments();
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }

    
}