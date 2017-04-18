using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Permissions.Commands
{
    [TestFixture]
    public class LoadDataOnPersonLightCommandTest
    {
        private MockRepository _mocks;
        private  IUnitOfWorkFactory _unitOfWorkFactory;
        private  IRepositoryFactory _repositoryFactory;
        private  IPermissionViewerRoles _permissionViewerRoles;
        private LoadDataOnPersonsLightCommand _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _permissionViewerRoles = _mocks.StrictMock<IPermissionViewerRoles>();
            _target = new LoadDataOnPersonsLightCommand(_unitOfWorkFactory, _repositoryFactory, _permissionViewerRoles);
        }

        [Test]
        public void ShouldGetRolesFromRepAndLoadListView()
        {
            var nodeId = Guid.NewGuid();
            var nodeOne = new TreeNodeAdv {TagObject = nodeId, ShowCheckBox = true};
            var nodeTwo = new TreeNodeAdv {TagObject = 3, ShowCheckBox = true};
            var lst = new [] {nodeOne, nodeTwo};
            var id = Guid.NewGuid();
            
            var uow = _mocks.StrictMock<IStatelessUnitOfWork>();
            var rep = _mocks.StrictMock<IApplicationRolePersonRepository>();
            Expect.Call(_permissionViewerRoles.SelectedPerson).Return(id);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateApplicationRolePersonRepository(uow)).Return(rep);
            Expect.Call(rep.AvailableData(id)).Return(new List<Guid> { nodeId });
            Expect.Call(rep.DataRangeOptions(id)).Return(new List<int> {3});
            Expect.Call(_permissionViewerRoles.AllDataNodes).Return(lst);
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            _target.Execute();
            Assert.That(lst[0].Checked, Is.True);
            _mocks.VerifyAll();
        }
    }

    
}