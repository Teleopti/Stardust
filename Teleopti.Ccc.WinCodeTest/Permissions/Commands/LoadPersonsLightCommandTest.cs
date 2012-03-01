using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.Permissions;
using Teleopti.Ccc.WinCode.Permissions.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Permissions.Commands
{
    [TestFixture]
    public class LoadPersonsLightCommandTest
    {
        private MockRepository _mocks;
        private  IUnitOfWorkFactory _unitOfWorkFactory;
        private  IRepositoryFactory _repositoryFactory;
        private  IPermissionViewerRoles _permissionViewerRoles;
        private LoadPersonsLightCommand _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _permissionViewerRoles = _mocks.StrictMock<IPermissionViewerRoles>();
            _target = new LoadPersonsLightCommand(_unitOfWorkFactory, _repositoryFactory, _permissionViewerRoles);
        }

        [Test]
        public void ShouldGetPersonsFromRepAndLoadListView()
        {
            var uow = _mocks.StrictMock<IStatelessUnitOfWork>();
            var rep = _mocks.StrictMock<IApplicationRolePersonRepository>();
            var list = new ListView();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateApplicationRolePersonRepository(uow)).Return(rep);
            Expect.Call(rep.Persons()).Return(new List<IPersonInRole> { new PersonInRole { Id = Guid.NewGuid(), FirstName = "Admin" } });
            Expect.Call(_permissionViewerRoles.PersonsMainList).Return(list);
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            _target.Execute();
            Assert.That(list.Items.Count, Is.EqualTo(1));
            _mocks.VerifyAll();
        }
    }

    
}