using System;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
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
        private ILoadDataOnPersonsLightCommand _loadDataOnPersonsLightCommand;
        private ILoadRolesAndPersonsOnDataLightCommand _loadRolesAndPersonsOnDataLightCommand;

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
            _loadDataOnPersonsLightCommand = _mocks.StrictMock<ILoadDataOnPersonsLightCommand>();
            _loadRolesAndPersonsOnDataLightCommand = _mocks.StrictMock<ILoadRolesAndPersonsOnDataLightCommand>();
            _eventAggregator = new EventAggregator();
            _view = _mocks.StrictMock<IPermissionViewerRoles>();
            _target = new PermissionViewerRolesPresenter(_eventAggregator, _loadRolesCommand, _view, _loadPersonsCommand, _loadRolesOnPersonCommand,
                _loadFunctionsOnPersonLightCommand, _loadFunctionsLightCommand, _loadPersonsWithFunctionLightCommand, _loadRolesWithFunctionLightCommand,
                _loadDataOnPersonsLightCommand, _loadRolesAndPersonsOnDataLightCommand);
        }

        [Test]
        public void ShouldExecuteLoadsOnPersonWhenNeeded()
        {
            Expect.Call(_view.SelectedPerson).Return(Guid.Empty);
            Expect.Call(_view.SelectedPerson).Return(Guid.NewGuid());
            Expect.Call(_loadRolesOnPersonCommand.Execute);
            Expect.Call(_loadFunctionsOnPersonLightCommand.Execute);
            Expect.Call(_loadDataOnPersonsLightCommand.Execute);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<PersonRolesAndFunctionsNeedLoad>().Publish("");
            _eventAggregator.GetEvent<PersonRolesAndFunctionsNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldExecuteLoadsOnFunctionWhenNeeded()
        {
            Expect.Call(_view.SelectedFunction).Return(Guid.Empty);
            Expect.Call(_view.SelectedFunction).Return(Guid.NewGuid());
            Expect.Call(_loadPersonsWithFunctionLightCommand.Execute);
            Expect.Call(_loadRolesWithFunctionLightCommand.Execute);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Publish("");
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }
        [Test]
        public void ShouldLoadSomeOnShow()
        {
            Expect.Call(_loadRolesCommand.Execute);
            Expect.Call(_loadPersonsCommand.Execute);
            Expect.Call(_loadFunctionsLightCommand.Execute);
            Expect.Call(() => _view.FillDataTree(new TreeNodeAdv[0], new TreeNodeAdv[0], new TreeNodeAdv[0])).IgnoreArguments();
            Expect.Call(_view.Show);
            Expect.Call(_view.BringToFront);
            _mocks.ReplayAll();
            _target.ShowViewer();
            _mocks.VerifyAll();
        }
    }

    
}