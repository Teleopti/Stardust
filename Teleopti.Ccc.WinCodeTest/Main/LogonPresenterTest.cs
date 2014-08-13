using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Main
{
    [TestFixture]
	public class LogonPresenterTest
	{
        private MockRepository _mocks;
        private ILogonView _view;
        private LogonModel _model;
        private IDataSourceHandler _dataSourceHandler;
        private ILoginInitializer _initializer;
        private ILogonLogger _logonLogger;
        private ILogOnOff _logOnOff;
        private LogonPresenter _target;
        private IServerEndpointSelector _endPointSelector;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.DynamicMock<ILogonView>();
            _model = new LogonModel();
            _dataSourceHandler = _mocks.DynamicMock<IDataSourceHandler>();
            _initializer = _mocks.DynamicMock<ILoginInitializer>();
            _logonLogger = _mocks.DynamicMock<ILogonLogger>();
            _logOnOff = _mocks.DynamicMock<ILogOnOff>();
            _endPointSelector = _mocks.DynamicMock<IServerEndpointSelector>();
            _target = new LogonPresenter(_view, _model, _dataSourceHandler, _initializer, _logonLogger, _logOnOff, _endPointSelector);
        }

        [Test]
        public void ShouldCallStartLogonOnViewAtStartUp()
        {
	        Expect.Call(_view.StartLogon()).Return(true);
            _mocks.ReplayAll();
            _target.Start();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadAvailableSdksOnStepOne()
        {
            Expect.Call(() => _view.ClearForm("")).IgnoreArguments();
            Expect.Call(_endPointSelector.GetEndpointNames()).Return(new List<string> {"local", "local2"});
            Expect.Call(() => _view.ShowStep(false));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectSdk;
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGoToDataSourcesIfOneSdk()
        {
            Expect.Call(() => _view.ClearForm("")).IgnoreArguments();
            Expect.Call(_endPointSelector.GetEndpointNames()).Return(new List<string> { "local" });
            Expect.Call(_dataSourceHandler.DataSourceProviders()).Return(new List<IDataSourceProvider>());
            Expect.Call(() => _view.ShowStep(false));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectSdk;
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetDataSources()
        {
            var dataSorceProvider = _mocks.DynamicMock<IDataSourceProvider>();
            var dataSourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);
            _model.Sdks = new List<string>{"sdk1", "sdk2"};
            _model.SelectedSdk = "sdk1";
            Expect.Call(() => _view.ClearForm("")).IgnoreArguments();
            Expect.Call(_dataSourceHandler.DataSourceProviders()).Return(new List<IDataSourceProvider> { dataSorceProvider });
            Expect.Call(dataSorceProvider.DataSourceList()).Return(new List<DataSourceContainer> {dataSourceContainer});
            Expect.Call(() => _view.ShowStep(true));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectSdk;
            _target.OkbuttonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReloadSdkOnBackFromDataSources()
        {
			_model.SelectedDataSourceContainer = new DataSourceContainer(null,null,null, AuthenticationTypeOption.Application);
            Expect.Call(() => _view.ClearForm("")).IgnoreArguments();
            Expect.Call(_endPointSelector.GetEndpointNames()).Return(new List<string> { "local", "local2" });
            Expect.Call(() => _view.ShowStep(false));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectDatasource;
            _target.BackButtonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void LogonModelShouldHaveValidLogin()
        {
            Assert.That(_model.HasValidLogin(), Is.False);
            _model.Password = "p";
            Assert.That(_model.HasValidLogin(), Is.False);
            _model.Password = "";
            _model.UserName = "u";
            Assert.That(_model.HasValidLogin(), Is.False);
            _model.Password = "p";
            Assert.That(_model.HasValidLogin(), Is.True);
        }

        [Test]
        public void ShouldNotReloadDataSourcesOnBack()
        {
            var dataSourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);
            _model.Sdks = new List<string> { "sdk1", "sdk2" };
            _model.SelectedSdk = "sdk1";
            _model.SelectedDataSourceContainer = dataSourceContainer;
            _model.DataSourceContainers = new List<IDataSourceContainer>{dataSourceContainer};
            Expect.Call(() => _view.ShowStep(true));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.Login;
            _target.BackButtonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSaveLoginAttempOnLogin()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFact = _mocks.DynamicMock<IUnitOfWorkFactory>();
            _model.SelectedDataSourceContainer = dataSourceContainer;
            _model.UserName = "USER";
            _model.Password = "PASS";
            Expect.Call(dataSourceContainer.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
            Expect.Call(dataSourceContainer.LogOn("USER", "PASS"))
                  .Return(new AuthenticationResult {HasMessage = true, Message = "ERRRRROR"});
            Expect.Call(() => _view.ShowErrorMessage("ERRRRROR  ",Resources.ErrorMessage));
            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFact);
            Expect.Call(() =>_logonLogger.SaveLogonAttempt(new LoginAttemptModel(), uowFact)).IgnoreArguments();
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.Login;
            _target.OkbuttonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetBusAfterLogin()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var person = _mocks.DynamicMock<IPerson>();
            var appAuthInfo = _mocks.DynamicMock<IApplicationAuthenticationInfo>();
            var buProvider = _mocks.DynamicMock<IAvailableBusinessUnitsProvider>();
            _model.SelectedDataSourceContainer = dataSourceContainer;
            _model.UserName = "USER";
            _model.Password = "PASS";
            
            Expect.Call(dataSourceContainer.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
            Expect.Call(dataSourceContainer.LogOn("USER", "PASS"))
                  .Return(new AuthenticationResult {Successful = true});
            Expect.Call(dataSourceContainer.User).Return(person);
            Expect.Call(person.ApplicationAuthenticationInfo).Return(appAuthInfo);
            Expect.Call(appAuthInfo.Password = "PASS");
            Expect.Call(dataSourceContainer.AvailableBusinessUnitProvider).Return(buProvider);
            Expect.Call(buProvider.AvailableBusinessUnits()).Return(new List<IBusinessUnit>());
            Expect.Call(() => _view.ShowErrorMessage(Resources.NoAllowedBusinessUnitFoundInCurrentDatabase, Resources.ErrorMessage));
            Expect.Call(() => _view.ShowStep(true));
           _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.Login;
            _target.OkbuttonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetBusAfterLoginIfOneSkipSelect()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFact = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var person = _mocks.DynamicMock<IPerson>();
            var appAuthInfo = _mocks.DynamicMock<IApplicationAuthenticationInfo>();
            var buProvider = _mocks.DynamicMock<IAvailableBusinessUnitsProvider>();
            var bu = new BusinessUnit("Bu One");
            _model.SelectedDataSourceContainer = dataSourceContainer;
            _model.UserName = "USER";
            _model.Password = "PASS";
            _model.SelectedBu = bu;
            Expect.Call(dataSourceContainer.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFact);
            Expect.Call(dataSourceContainer.LogOn("USER", "PASS"))
                  .Return(new AuthenticationResult { Successful = true });
            Expect.Call(dataSourceContainer.User).Return(person);
            Expect.Call(person.ApplicationAuthenticationInfo).Return(appAuthInfo);
            Expect.Call(appAuthInfo.Password = "PASS");
            Expect.Call(dataSourceContainer.AvailableBusinessUnitProvider).Return(buProvider);
            Expect.Call(buProvider.AvailableBusinessUnits()).Return(new List<IBusinessUnit>{bu});
            Expect.Call(() => _view.ClearForm(Resources.InitializingTreeDots));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.Login;
            _target.OkbuttonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGoToLoginIfApplication()
        {
            var dataSourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);
			var availableDataSourcesProvider = _mocks.DynamicMock<IAvailableDataSourcesProvider>();
            _model.SelectedDataSourceContainer = dataSourceContainer;
            _model.DataSourceContainers = new List<IDataSourceContainer> { dataSourceContainer };

			Expect.Call(_dataSourceHandler.AvailableDataSourcesProvider()).Return(availableDataSourcesProvider);
			Expect.Call(availableDataSourcesProvider.UnavailableDataSources())
				  .Return(new List<IDataSource>());
            Expect.Call(() => _view.ShowStep(true));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectDatasource;
            _target.OkbuttonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldInitAppAfterSelectBus()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFact = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var person = _mocks.DynamicMock<IPerson>();
            var buProvider = _mocks.DynamicMock<IAvailableBusinessUnitsProvider>();
            var bu = new BusinessUnit("Bu One");
            _model.SelectedDataSourceContainer = dataSourceContainer;
            _model.SelectedBu = bu;

            Expect.Call(() => _view.ClearForm(Resources.InitializingTreeDots));
            Expect.Call(dataSourceContainer.AvailableBusinessUnitProvider).Return(buProvider);
            Expect.Call(buProvider.LoadHierarchyInformation(bu)).Return(bu);
            Expect.Call(dataSourceContainer.User).Return(person);
            Expect.Call(() =>_logOnOff.LogOn(dataSource, person, bu));
            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFact);
            Expect.Call(() => _logonLogger.SaveLogonAttempt(new LoginAttemptModel(), uowFact)).IgnoreArguments();
            Expect.Call(_initializer.InitializeApplication(dataSourceContainer)).Return(true);
            Expect.Call(() => _view.Exit(DialogResult.OK));

            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectBu;
            _target.OkbuttonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldExitWithCancelIfInitIsFalse()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFact = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var person = _mocks.DynamicMock<IPerson>();
            var buProvider = _mocks.DynamicMock<IAvailableBusinessUnitsProvider>();
            var bu = new BusinessUnit("Bu One");
            _model.SelectedDataSourceContainer = dataSourceContainer;
            _model.SelectedBu = bu;

            Expect.Call(() => _view.ClearForm(Resources.InitializingTreeDots));
            Expect.Call(dataSourceContainer.AvailableBusinessUnitProvider).Return(buProvider);
            Expect.Call(buProvider.LoadHierarchyInformation(bu)).Return(bu);
            Expect.Call(dataSourceContainer.User).Return(person);
            Expect.Call(() => _logOnOff.LogOn(dataSource, person, bu));
            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFact);
            Expect.Call(() => _logonLogger.SaveLogonAttempt(new LoginAttemptModel(), uowFact)).IgnoreArguments();
            Expect.Call(_initializer.InitializeApplication(dataSourceContainer)).Return(false);
            Expect.Call(() => _view.Exit(DialogResult.Cancel));

            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectBu;
            _target.OkbuttonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetBusAfterDataSourcesIfWindows()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var buProvider = _mocks.DynamicMock<IAvailableBusinessUnitsProvider>();
			var availableDataSourcesProvider = _mocks.DynamicMock<IAvailableDataSourcesProvider>();

            var bu = new BusinessUnit("Bu One");
            var bu2 = new BusinessUnit("Bu two");
            _model.SelectedDataSourceContainer = dataSourceContainer;
	        _model.DataSourceContainers = new List<IDataSourceContainer> {dataSourceContainer};
			_model.Sdks = new List<string> {"test sdk", "test sdk2"};

	        Expect.Call(_dataSourceHandler.AvailableDataSourcesProvider()).Return(availableDataSourcesProvider);
	        Expect.Call(availableDataSourcesProvider.UnavailableDataSources())
	              .Return(new List<IDataSource>());
			Expect.Call(dataSourceContainer.AuthenticationTypeOption).Return(AuthenticationTypeOption.Windows);
            Expect.Call(dataSourceContainer.AvailableBusinessUnitProvider).Return(buProvider);
            Expect.Call(buProvider.AvailableBusinessUnits()).Return(new List<IBusinessUnit> { bu, bu2 });
            Expect.Call(() =>_view.ShowStep(true));
           
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectDatasource;
            _target.OkbuttonClicked();
            _mocks.VerifyAll();
        }
	}

    
}
