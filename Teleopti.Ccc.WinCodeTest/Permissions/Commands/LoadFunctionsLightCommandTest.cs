using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Permissions.Commands
{
    [TestFixture]
    public class LoadFunctionsLightCommandTest
    {
        private MockRepository _mocks;
        private  IUnitOfWorkFactory _unitOfWorkFactory;
        private  IRepositoryFactory _repositoryFactory;
        private  IPermissionViewerRoles _permissionViewerRoles;
        private LoadFunctionsLightCommand _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _permissionViewerRoles = _mocks.StrictMock<IPermissionViewerRoles>();
            _target = new LoadFunctionsLightCommand(_unitOfWorkFactory, _repositoryFactory, _permissionViewerRoles);
        }

        [Test]
        public void ShouldGetFunctionsFromRepAndLoadListView()
        {
           var uow = _mocks.StrictMock<IStatelessUnitOfWork>();
            var rep = _mocks.StrictMock<IApplicationRolePersonRepository>();
            var rootId = Guid.NewGuid();
            var funcRoot = new FunctionLight { Id = rootId, Name = "Root", ResourceName = "xxNgt", Parent = Guid.Empty};
            var funcNext = new FunctionLight { Id = Guid.NewGuid(), Name = "Admin", ResourceName = "xxNgt", Parent = rootId};

            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateApplicationRolePersonRepository(uow)).Return(rep);
            Expect.Call(rep.Functions()).Return(new List<IFunctionLight> { funcRoot, funcNext });
            Expect.Call(() => _permissionViewerRoles.FillFunctionTree(new TreeNodeAdv[0], new TreeNodeAdv[0], new TreeNodeAdv[0])).IgnoreArguments();

            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }
}