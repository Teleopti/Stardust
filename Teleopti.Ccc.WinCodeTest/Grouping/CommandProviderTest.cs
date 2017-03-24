using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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
        private CommandProvider _target;
        private IPersonSelectorView _view;
        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
                                           DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);


	    [SetUp]
        public void Setup()
	    {
		    _view = MockRepository.GenerateMock<IPersonSelectorView>();
		    _target = new CommandProvider(MockRepository.GenerateMock<IUnitOfWorkFactory>(),
		                                  MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>(),
		                                  MockRepository.GenerateMock<IGlobalSettingDataRepository>(), _view);
	    }

	    [Test]
        public void ShouldDeliverLoadOrganizationCommand()
        {
            Assert.That(_target.GetLoadOrganizationCommand(_myApplicationFunction,true, true),Is.Not.Null);
        }

        [Test]
        public void ShouldDeliverLoadBuiltInCommand()
        {
            Assert.That(_target.GetLoadBuiltInTabsCommand(PersonSelectorField.Contract,_view, "Contract" ,_myApplicationFunction, Guid.Empty), Is.Not.Null);
        }

        [Test]
        public void ShouldDeliverLoadUserDefinedCommand()
        {
            Assert.That(_target.GetLoadUserDefinedTabsCommand( _view, Guid.NewGuid(), _myApplicationFunction), Is.Not.Null);
        }
    }

}