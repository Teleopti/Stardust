using System;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Permissions
{
    [TestFixture]
    public class PermissionViewerRolesPresenterTest
    {
        private MockRepository _mocks;
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
        private ILoadRolesAndPersonsOnDataRangeLightCommand _loadRolesAndPersonsOnDataRangeLightCommand;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _loadPersonsCommand = _mocks.StrictMock<ILoadPersonsLightCommand>();
            _loadRolesOnPersonCommand = _mocks.StrictMock<ILoadRolesOnPersonLightCommand>();
            _loadFunctionsOnPersonLightCommand = _mocks.StrictMock<ILoadFunctionsOnPersonLightCommand>();
            _loadFunctionsLightCommand = _mocks.StrictMock<ILoadFunctionsLightCommand>();
            _loadPersonsWithFunctionLightCommand = _mocks.StrictMock<ILoadPersonsWithFunctionLightCommand>();
            _loadRolesWithFunctionLightCommand = _mocks.StrictMock<ILoadRolesWithFunctionLightCommand>();
            _loadDataOnPersonsLightCommand = _mocks.StrictMock<ILoadDataOnPersonsLightCommand>();
            _loadRolesAndPersonsOnDataLightCommand = _mocks.StrictMock<ILoadRolesAndPersonsOnDataLightCommand>();
            _loadRolesAndPersonsOnDataRangeLightCommand =
                _mocks.StrictMock<ILoadRolesAndPersonsOnDataRangeLightCommand>();
            _eventAggregator = new EventAggregator();
            _view = _mocks.StrictMock<IPermissionViewerRoles>();
            _target = new PermissionViewerRolesPresenter(_eventAggregator, _view, _loadPersonsCommand, _loadRolesOnPersonCommand,
                _loadFunctionsOnPersonLightCommand, _loadFunctionsLightCommand, _loadPersonsWithFunctionLightCommand, _loadRolesWithFunctionLightCommand,
                _loadDataOnPersonsLightCommand, _loadRolesAndPersonsOnDataLightCommand, _loadRolesAndPersonsOnDataRangeLightCommand);
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
        public void ShouldLoadRolesAndPersonsOnData()
        {
            Expect.Call(_loadRolesAndPersonsOnDataLightCommand.Execute);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<DataPersonsAndRolesNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadRolesAndPersonsOnDataRange()
        {
            Expect.Call(_loadRolesAndPersonsOnDataRangeLightCommand.Execute);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<DataRangePersonsAndRolesNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadSomeOnShow()
        {
            var site = new Site("sajt");
            var deletedSite = new Site("sajt");
            deletedSite.SetDeleted();
            var deletedTeam = new Team();
            deletedTeam.SetDeleted();
            site.AddTeam(new Team());
            site.AddTeam(deletedTeam);
            IBusinessUnit bu = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit;
            bu.AddSite(site);
            bu.AddSite(deletedSite);
            Expect.Call(_loadPersonsCommand.Execute);
            Expect.Call(_loadFunctionsLightCommand.Execute);
            Expect.Call(() => _view.FillDataTree(new TreeNodeAdv[0], new TreeNodeAdv[0], new TreeNodeAdv[0])).IgnoreArguments();
            Expect.Call(_view.Show);
            Expect.Call(_view.BringToFront);
            _mocks.ReplayAll();
            _target.ShowViewer();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetUnloaded()
        {
            Assert.That(_target.Unloaded, Is.False);
            _eventAggregator.GetEvent<PermissionsViewerUnloaded>().Publish("");
            Assert.That(_target.Unloaded, Is.True);
        }

        [Test]
        public void ShouldCloseForm()
        {
            Expect.Call(_view.Close);
            _mocks.ReplayAll();
            _target.CloseViewer();
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "errrorrrrrr"), Test]
        public void ShouldCallViewToShowErrorOnPerson()
        {
            var err = new Infrastructure.Foundation.DataSourceException("errrorrrrrr");
            Expect.Call(_view.SelectedPerson).Return(Guid.NewGuid());
            Expect.Call(_loadRolesOnPersonCommand.Execute).Throw(err);
            Expect.Call(() => _view.ShowDataSourceException(err));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<PersonRolesAndFunctionsNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "errrorrrrrr"), Test]
        public void ShouldCallViewToShowErrorOnFunction()
        {
            var err = new Infrastructure.Foundation.DataSourceException("errrorrrrrr");
            Expect.Call(_view.SelectedFunction).Return(Guid.NewGuid());
            Expect.Call(_loadPersonsWithFunctionLightCommand.Execute).Throw(err);
            Expect.Call(() => _view.ShowDataSourceException(err));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "errrorrrrrr"), Test]
        public void ShouldCallViewToShowErrorOnDataRange()
        {
            var err = new Infrastructure.Foundation.DataSourceException("errrorrrrrr");
            Expect.Call(_loadRolesAndPersonsOnDataRangeLightCommand.Execute).Throw(err);
            Expect.Call(() =>_view.ShowDataSourceException(err));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<DataRangePersonsAndRolesNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "errrorrrrrr"), Test]
        public void ShouldCallViewToShowErrorOnData()
        {
            var err = new Infrastructure.Foundation.DataSourceException("errrorrrrrr");
            Expect.Call(_loadRolesAndPersonsOnDataLightCommand.Execute).Throw(err);
            Expect.Call(() => _view.ShowDataSourceException(err));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<DataPersonsAndRolesNeedLoad>().Publish("");
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "errrorrrrrr"), Test]
        public void ShouldCallViewToShowErrorOnShow()
        {
            var err = new Infrastructure.Foundation.DataSourceException("errrorrrrrr");
            
            Expect.Call(_loadPersonsCommand.Execute).Throw(err);
            Expect.Call(() => _view.ShowDataSourceException(err));
            _mocks.ReplayAll();
            _target.ShowViewer();
            _mocks.VerifyAll();
        }
    }

    
}