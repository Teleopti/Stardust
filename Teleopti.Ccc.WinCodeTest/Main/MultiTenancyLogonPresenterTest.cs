using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCodeTest.Main
{
	[TestFixture]
	public class MultiTenancyLogonPresenterTest
	{
		private ILogonView _view;
		private LogonModel _model;
		private IDataSourceHandler _dataSourceHandler;
		private ILoginInitializer _initializer;
		private ILogonLogger _logonLogger;
		private ILogOnOff _logOnOff;
		private MultiTenancyLogonPresenter _target;
		private IServerEndpointSelector _endPointSelector;
		private IMultiTenancyApplicationLogon _appLogon;
		private IMultiTenancyWindowsLogon _winLogon;

		[SetUp]
		public void Setup()
		{
			_view = MockRepository.GenerateMock<ILogonView>();
			_model = new LogonModel();
			_dataSourceHandler = MockRepository.GenerateMock<IDataSourceHandler>();
			_initializer = MockRepository.GenerateMock<ILoginInitializer>();
			_logonLogger = MockRepository.GenerateMock<ILogonLogger>();
			_logOnOff = MockRepository.GenerateMock<ILogOnOff>();
			_endPointSelector = MockRepository.GenerateMock<IServerEndpointSelector>();
			_appLogon = MockRepository.GenerateMock<IMultiTenancyApplicationLogon>();
			_winLogon = MockRepository.GenerateMock<IMultiTenancyWindowsLogon>();
			_target = new MultiTenancyLogonPresenter(_view, _model, _dataSourceHandler, _initializer, _logonLogger, _logOnOff,
				_endPointSelector, MockRepository.GenerateMock<IMessageBrokerComposite>(), _appLogon, _winLogon);
			_model.AuthenticationType = AuthenticationTypeOption.Application;
		}

		[Test]
		public void ShouldCallStartLogonOnViewAtStartUp()
		{
			_view.Stub(x => x.StartLogon(true)).Return(true);
			_target.Start();
		}

		[Test]
		public void ShouldLoadAvailableSdksOnStepOne()
		{
			_view.Stub(x => x.ClearForm("")).IgnoreArguments();
			_endPointSelector.Stub(x => x.GetEndpointNames()).Return(new List<string> { "local", "local2" });
			_view.Stub(x => x.ShowStep(false));
			_target.CurrentStep = LoginStep.SelectSdk;
			_target.Initialize();
		}

		[Test]
		public void ShouldGoToDataSourcesIfOneSdk()
		{
			_view.Stub(x => x.ClearForm("")).IgnoreArguments();
			_endPointSelector.Stub(x => x.GetEndpointNames()).Return(new List<string> { "local" });
			_dataSourceHandler.Stub(x => x.DataSourceProviders()).Return(new List<IDataSourceProvider>());
			_view.Stub(x => x.ShowStep(false));
			_target.CurrentStep = LoginStep.SelectSdk;
			_target.Initialize();
		}

		[Test]
		public void ShouldGetDataSources()
		{
			var dataSorceProvider = MockRepository.GenerateMock<IDataSourceProvider>();
			var dataSourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);
			_model.Sdks = new List<string> { "sdk1", "sdk2" };
			_model.SelectedSdk = "sdk1";
			_view.Stub(x => x.ClearForm("")).IgnoreArguments();
			_dataSourceHandler.Stub(x => x.DataSourceProviders()).Return(new List<IDataSourceProvider> { dataSorceProvider });
			dataSorceProvider.Stub(x => x.DataSourceList()).Return(new List<DataSourceContainer> { dataSourceContainer });
			_view.Stub(x => x.ShowStep(false));
			_target.CurrentStep = LoginStep.SelectSdk;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldNotReloadSdkOnBackFromDataSources()
		{
			_model.SelectedDataSourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);
			_target.CurrentStep = LoginStep.SelectDatasource;
			_target.BackButtonClicked();
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
			_model.DataSourceContainers = new List<IDataSourceContainer> { dataSourceContainer };
			_target.CurrentStep = LoginStep.Login;
			_target.BackButtonClicked();
		}

		[Test]
		public void ShouldSaveLoginAttempOnLogin()
		{
			var dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowFact = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.UserName = "USER";
			_model.Password = "PASS";
			_model.AuthenticationType = AuthenticationTypeOption.Application;
			_appLogon.Stub(x => x.Logon(_model)).Return(new AuthenticationResult { HasMessage = true, Message = "ERRRRROR" });
			_view.Stub(x => x.ShowErrorMessage("ERRRRROR  ", Resources.ErrorMessage));
			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(uowFact);
			_logonLogger.Stub(x => x.SaveLogonAttempt(new LoginAttemptModel(), uowFact)).IgnoreArguments();
			_target.CurrentStep = LoginStep.Login;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldGetBusAfterLogin()
		{
			var dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			var person = MockRepository.GenerateMock<IPerson>();
			var appAuthInfo = MockRepository.GenerateMock<IApplicationAuthenticationInfo>();
			var buProvider = MockRepository.GenerateMock<IAvailableBusinessUnitsProvider>();
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.UserName = "USER";
			_model.Password = "PASS";

			dataSourceContainer.Stub(x => x.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
			_appLogon.Stub(x => x.Logon(_model)).Return(new AuthenticationResult { Successful = true });

			dataSourceContainer.Stub(x => x.User).Return(person);
			person.Stub(x => x.ApplicationAuthenticationInfo).Return(appAuthInfo);
			appAuthInfo.Stub(x => x.Password = "PASS");
			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.AvailableBusinessUnits()).Return(new List<IBusinessUnit>());
			_view.Stub(x => x.ShowErrorMessage(Resources.NoAllowedBusinessUnitFoundInCurrentDatabase, Resources.ErrorMessage));
			_view.Stub(x => x.ShowStep(true));
			_target.CurrentStep = LoginStep.Login;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldGetBusAfterLoginIfOneSkipSelect()
		{
			var dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowFact = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var person = MockRepository.GenerateMock<IPerson>();
			var appAuthInfo = MockRepository.GenerateMock<IApplicationAuthenticationInfo>();
			var buProvider = MockRepository.GenerateMock<IAvailableBusinessUnitsProvider>();
			var bu = new BusinessUnit("Bu One");
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.UserName = "USER";
			_model.Password = "PASS";
			_model.SelectedBu = bu;
			dataSourceContainer.Stub(x => x.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(uowFact);
			_appLogon.Stub(x => x.Logon(_model)).Return(new AuthenticationResult { Successful = true });
			dataSourceContainer.Stub(x => x.User).Return(person);
			person.Stub(x => x.ApplicationAuthenticationInfo).Return(appAuthInfo);
			appAuthInfo.Stub(x => x.Password = "PASS");
			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.AvailableBusinessUnits()).Return(new List<IBusinessUnit> { bu });
			_view.Stub(x => x.ClearForm(Resources.InitializingTreeDots));
			_target.CurrentStep = LoginStep.Login;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldGoToLoginIfApplication()
		{
			var dataSourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);
			var availableDataSourcesProvider = MockRepository.GenerateMock<IAvailableDataSourcesProvider>();
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.DataSourceContainers = new List<IDataSourceContainer> { dataSourceContainer };

			_dataSourceHandler.Stub(x => x.AvailableDataSourcesProvider()).Return(availableDataSourcesProvider);
			availableDataSourcesProvider.Stub(x => x.UnavailableDataSources())
				  .Return(new List<IDataSource>());
			_view.Stub(x => x.ShowStep(false));
			_target.CurrentStep = LoginStep.SelectDatasource;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldInitAppAfterSelectBus()
		{
			var dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowFact = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var person = MockRepository.GenerateMock<IPerson>();
			var buProvider = MockRepository.GenerateMock<IAvailableBusinessUnitsProvider>();
			var bu = new BusinessUnit("Bu One");
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.SelectedBu = bu;

			_view.Stub(x => x.ClearForm(Resources.InitializingTreeDots));
			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.LoadHierarchyInformation(bu)).Return(bu);
			dataSourceContainer.Stub(x => x.User).Return(person);
			_logOnOff.Stub(x => x.LogOn(dataSource, person, bu));
			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(uowFact);
			_logonLogger.Stub(x => x.SaveLogonAttempt(new LoginAttemptModel(), uowFact)).IgnoreArguments();
			_initializer.Stub(x => x.InitializeApplication(dataSourceContainer)).Return(true);
			_view.Stub(x => x.Exit(DialogResult.OK));

			_target.CurrentStep = LoginStep.SelectBu;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldExitWithCancelIfInitIsFalse()
		{
			var dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowFact = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var person = MockRepository.GenerateMock<IPerson>();
			var buProvider = MockRepository.GenerateMock<IAvailableBusinessUnitsProvider>();
			var bu = new BusinessUnit("Bu One");
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.SelectedBu = bu;

			_view.Stub(x => x.ClearForm(Resources.InitializingTreeDots));
			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.LoadHierarchyInformation(bu)).Return(bu);
			dataSourceContainer.Stub(x => x.User).Return(person);
			_logOnOff.Stub(x => x.LogOn(dataSource, person, bu));
			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(uowFact);
			_logonLogger.Stub(x => x.SaveLogonAttempt(new LoginAttemptModel(), uowFact)).IgnoreArguments();
			_initializer.Stub(x => x.InitializeApplication(dataSourceContainer)).Return(false);
			_view.Stub(x => x.Exit(DialogResult.Cancel));

			_target.CurrentStep = LoginStep.SelectBu;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldGetBusAfterDataSourcesIfWindows()
		{
			var dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			var buProvider = MockRepository.GenerateMock<IAvailableBusinessUnitsProvider>();
			var availableDataSourcesProvider = MockRepository.GenerateMock<IAvailableDataSourcesProvider>();
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			var bu = new BusinessUnit("Bu One");
			var bu2 = new BusinessUnit("Bu two");
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.DataSourceContainers = new List<IDataSourceContainer> { dataSourceContainer };
			_model.Sdks = new List<string> { "test sdk", "test sdk2" };

			_dataSourceHandler.Stub(x => x.AvailableDataSourcesProvider()).Return(availableDataSourcesProvider);
			availableDataSourcesProvider.Stub(x => x.UnavailableDataSources()).Return(new List<IDataSource>());
			_winLogon.Stub(x => x.Logon(_model)).Return(new AuthenticationResult {Successful = true});
			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.AvailableBusinessUnits()).Return(new List<IBusinessUnit> { bu, bu2 });
			_view.Stub(x => x.ShowStep(true));

			_target.CurrentStep = LoginStep.SelectDatasource;
			_target.OkbuttonClicked();
		}
	}


}
