using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Grouping
{
    [TestFixture]
    public class CommandProviderTest
    {
        private MockRepository _mocks;
        private CommandProvider _target;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repFactory;
        private IPersonSelectorView _view;
        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                                           DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repFactory = _mocks.StrictMock<IRepositoryFactory>();
            _view = _mocks.StrictMock<IPersonSelectorView>();
            _target = new CommandProvider(_unitOfWorkFactory, _repFactory, _view);
        }

        [Test]
        public void ShouldDeliverLoadOrganizationCommand()
        {
            Assert.That(_target.GetLoadOrganizationCommand(_myApplicationFunction,true, true),Is.Not.Null);
        }

        [Test]
        public void ShouldDeliverLoadBuiltInCommand()
        {
            Assert.That(_target.GetLoadBuiltInTabsCommand(PersonSelectorField.Contract,_view, "Contract" ,_myApplicationFunction, true), Is.Not.Null);
        }

        [Test]
        public void ShouldDeliverLoadUserDefinedCommand()
        {
            Assert.That(_target.GetLoadUserDefinedTabsCommand( _view, Guid.NewGuid(), _myApplicationFunction, true), Is.Not.Null);
        }
    }

}