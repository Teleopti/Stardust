using System;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Main
{
	[TestFixture]
	public class LogonPresenterTest
	{
		private ILogonView _view;
		private LogonModel _model;
		private ILoginInitializer _initializer;
		private ILogOnOff _logOnOff;
		private LogonPresenter _target;
		private IMessageBrokerComposite _mBroker;
		private ISharedSettingsQuerier _sharedSettingsQuerier;
		private IAuthenticationQuerier _authenticationQuerier;
		private IAvailableBusinessUnitsProvider _availableBusinessUnitsProvider;

		[SetUp]
		public void Setup()
		{
			_view = MockRepository.GenerateMock<ILogonView>();
			_model = new LogonModel();
			_initializer = MockRepository.GenerateMock<ILoginInitializer>();
			_logOnOff = MockRepository.GenerateMock<ILogOnOff>();
			_authenticationQuerier = MockRepository.GenerateMock<IAuthenticationQuerier>();
			_mBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			_sharedSettingsQuerier = MockRepository.GenerateMock<ISharedSettingsQuerier>();
			_availableBusinessUnitsProvider = MockRepository.GenerateMock<IAvailableBusinessUnitsProvider>();
			_target = new LogonPresenter(_view, _model, _initializer,  _logOnOff,
				_mBroker, _sharedSettingsQuerier, _authenticationQuerier, new EnvironmentWindowsUserProvider(), _availableBusinessUnitsProvider);
			_model.AuthenticationType = AuthenticationTypeOption.Application;
		}

		[Test]
		public void ShouldCallStartLogonOnViewAtStartUp()
		{
			_view.Stub(x => x.StartLogon()).Return(true);
			_target.Start("raptor");
		}


		[Test]
		public void ShouldNotReloadSdkOnBackFromDataSources()
		{
			_model.SelectedDataSourceContainer = new DataSourceContainer(null, null);
			_target.CurrentStep = LoginStep.SelectLogonType;
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
		public void ShouldGetBusAfterLogin()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var appAuthInfo = new AuthenticationQuerierResult{Success = true,Person = person, DataSource = dataSource};
			
			_model.UserName = "USER";
			_model.Password = "PASS";

			_authenticationQuerier.Stub(x => x.TryLogon(new ApplicationLogonClientModel(),"WIN")).Return(appAuthInfo).IgnoreArguments();
			_availableBusinessUnitsProvider.Stub(x => x.AvailableBusinessUnits(person, dataSource)).Return(Enumerable.Empty<IBusinessUnit>());

			
			_view.Stub(x => x.ShowStep(true));
			_target.CurrentStep = LoginStep.Login;
			_target.OkbuttonClicked();
			_view.AssertWasCalled(x => x.ShowErrorMessage(Resources.NoAllowedBusinessUnitFoundInCurrentDatabase, Resources.ErrorMessage));
		}

		[Test]
		public void ShouldGoToLoginIfApplication()
		{
			var dataSourceContainer = new DataSourceContainer(null, null);
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_view.Stub(x => x.ShowStep(false));
			_target.CurrentStep = LoginStep.SelectLogonType;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldInitAppAfterSelectBus()
		{
			var dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var person = MockRepository.GenerateMock<IPerson>();
			var bu = new BusinessUnit("Bu One");
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.SelectedBu = bu;

			_view.Stub(x => x.ClearForm(Resources.InitializingTreeDots));
			_availableBusinessUnitsProvider.Stub(x => x.LoadHierarchyInformation(dataSource, bu)).Return(bu);
			dataSourceContainer.Stub(x => x.User).Return(person);
			_logOnOff.Stub(x => x.LogOnWithoutPermissions(dataSource, person, bu));
			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
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
			var person = MockRepository.GenerateMock<IPerson>();
			var bu = new BusinessUnit("Bu One");
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.SelectedBu = bu;

			_view.Stub(x => x.ClearForm(Resources.InitializingTreeDots));
			_availableBusinessUnitsProvider.Stub(x => x.LoadHierarchyInformation(dataSource, bu)).Return(bu);
			dataSourceContainer.Stub(x => x.User).Return(person);
			_logOnOff.Stub(x => x.LogOnWithoutPermissions(dataSource, person, bu));
			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			_initializer.Stub(x => x.InitializeApplication(dataSourceContainer)).Return(false);
			_view.Stub(x => x.Exit(DialogResult.Cancel));

			_target.CurrentStep = LoginStep.SelectBu;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldGetBusAfterDataSourcesIfWindows()
		{
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			var bu = new BusinessUnit("Bu One");
			var bu2 = new BusinessUnit("Bu two");
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var appAuthInfo = new AuthenticationQuerierResult { Success = true, Person = person, DataSource = dataSource };
			
			_authenticationQuerier.Stub(x => x.TryLogon(new IdentityLogonClientModel(), "WIN")).Return(appAuthInfo).IgnoreArguments();
			_availableBusinessUnitsProvider.Stub(x => x.AvailableBusinessUnits(person, dataSource)).Return(new[]{bu, bu2});

			_view.Stub(x => x.ShowStep(true));

			_target.CurrentStep = LoginStep.SelectLogonType;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldSetTenantCredentialsAfterWinAuthentication()
		{
			WinTenantCredentials.Clear();
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			var tenantPassword = RandomName.Make();
			var personId = Guid.NewGuid();
			var person = new Person();
			person.SetId(personId);
			var appAuthInfo = new AuthenticationQuerierResult { Success = true, Person = person, TenantPassword = tenantPassword};

			_availableBusinessUnitsProvider.Stub(x => x.AvailableBusinessUnits(null, null)).IgnoreArguments().Return(new[] { new BusinessUnit("_") });
			_authenticationQuerier.Stub(x => x.TryLogon(new IdentityLogonClientModel(), "WIN")).Return(appAuthInfo).IgnoreArguments();

			_target.CurrentStep = LoginStep.SelectBu;
			_target.OkbuttonClicked();

			var credentials = new WinTenantCredentials().TenantCredentials();
			credentials.PersonId.Should().Be.EqualTo(personId);
			credentials.TenantPassword.Should().Be.EqualTo(tenantPassword);
		}

		[Test]
		public void ShouldSetTenantCredentialsAfterApplicationAuthentication()
		{
			WinTenantCredentials.Clear();
			_model.AuthenticationType = AuthenticationTypeOption.Application;
			var tenantPassword = RandomName.Make();
			var personId = Guid.NewGuid();
			var person = new Person();
			person.SetId(personId);
			var appAuthInfo = new AuthenticationQuerierResult { Success = true, Person = person, TenantPassword = tenantPassword };

			_availableBusinessUnitsProvider.Stub(x => x.AvailableBusinessUnits(null, null)).IgnoreArguments().Return(new[] { new BusinessUnit("_") });
			_authenticationQuerier.Stub(x => x.TryLogon(new ApplicationLogonClientModel(), "APP")).Return(appAuthInfo).IgnoreArguments();

			_target.CurrentStep = LoginStep.SelectBu;
			_target.OkbuttonClicked();

			var credentials = new WinTenantCredentials().TenantCredentials();
			credentials.PersonId.Should().Be.EqualTo(personId);
			credentials.TenantPassword.Should().Be.EqualTo(tenantPassword);
		}

		[Test]
		public void ShouldGoDirectToApplicationIfWindowsNotPossible()
		{
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			_availableBusinessUnitsProvider.Stub(x => x.AvailableBusinessUnits(null, null)).IgnoreArguments().Return(new[] { new BusinessUnit("_") });
			_authenticationQuerier.Stub(x => x.TryLogon(new IdentityLogonClientModel(), null)).IgnoreArguments().Return(new AuthenticationQuerierResult{Success = false});

			_target.Initialize();
			_target.CurrentStep.Should().Be(LoginStep.Login);
			_model.AuthenticationType.Should().Be.EqualTo(AuthenticationTypeOption.Application);
		}
	}
}
