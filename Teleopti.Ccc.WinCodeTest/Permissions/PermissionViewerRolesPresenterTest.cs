using System;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Meetings.Events;
using Teleopti.Ccc.WinCode.Permissions;
using Teleopti.Ccc.WinCode.Permissions.Commands;
using Teleopti.Ccc.WinCode.Permissions.Events;

namespace Teleopti.Ccc.WinCodeTest.Permissions
{
    [TestFixture]
    public class PermissionViewerRolesPresenterTest
    {
        private MockRepository _mocks;
        private ILoadRolesLightCommand _loadRolesCommand;
        private EventAggregator _eventAggregator;
        private PermissionViewerRolesPresenter _target;
        private IPermissionViewerRoles _view;
        private ILoadPersonsLightCommand _loadPersonsCommand;
        private ILoadRolesOnPersonLightCommand _loadRolesOnPersonCommand;
        private ILoadFunctionsOnPersonLightCommand _loadFunctionsOnPersonLightCommand;
        private ILoadFunctionsLightCommand _loadFunctionsLightCommand;
        private ILoadPersonsWithFunctionLightCommand _loadPersonsWithFunctionLightCommand;
        private ILoadRolesWithFunctionLightCommand _loadRolesWithFunctionLightCommand;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _loadRolesCommand = _mocks.StrictMock<ILoadRolesLightCommand>();
            _loadPersonsCommand = _mocks.StrictMock<ILoadPersonsLightCommand>();
            _loadRolesOnPersonCommand = _mocks.StrictMock<ILoadRolesOnPersonLightCommand>();
            _loadFunctionsOnPersonLightCommand = _mocks.StrictMock<ILoadFunctionsOnPersonLightCommand>();
            _loadFunctionsLightCommand = _mocks.StrictMock<ILoadFunctionsLightCommand>();
            _loadPersonsWithFunctionLightCommand = _mocks.StrictMock<ILoadPersonsWithFunctionLightCommand>();
            _loadRolesWithFunctionLightCommand = _mocks.StrictMock<ILoadRolesWithFunctionLightCommand>();

            _eventAggregator = new EventAggregator();
            _view = _mocks.StrictMock<IPermissionViewerRoles>();
            _target = new PermissionViewerRolesPresenter(_eventAggregator, _loadRolesCommand, _view, _loadPersonsCommand, _loadRolesOnPersonCommand,
                _loadFunctionsOnPersonLightCommand, _loadFunctionsLightCommand, _loadPersonsWithFunctionLightCommand, _loadRolesWithFunctionLightCommand);
        }

        [Test]
        public void ShouldExecuteLoadsOnPersonWhenNeeded()
        {
            Expect.Call(_loadRolesOnPersonCommand.Execute);
            Expect.Call(_loadFunctionsOnPersonLightCommand.Execute);
            
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<PersonRolesAndFunctionsNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldExecuteLoadsOnFunctionWhenNeeded()
        {
            Expect.Call(_loadPersonsWithFunctionLightCommand.Execute);
            Expect.Call(_loadRolesWithFunctionLightCommand.Execute);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }
        [Test]
        public void ShouldLoadSomeOnShow()
        {
            Expect.Call(_loadRolesCommand.Execute);
            Expect.Call(_loadPersonsCommand.Execute);
            Expect.Call(_loadFunctionsLightCommand.Execute);
            Expect.Call(_view.Show);
            Expect.Call(_view.BringToFront);
            _mocks.ReplayAll();
            _target.ShowViewer();
            _mocks.VerifyAll();
        }
    }

    
}