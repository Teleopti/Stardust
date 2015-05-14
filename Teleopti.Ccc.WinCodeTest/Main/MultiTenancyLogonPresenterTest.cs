﻿using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
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
		private ILoginInitializer _initializer;
		private ILogOnOff _logOnOff;
		private MultiTenancyLogonPresenter _target;
		private IMultiTenancyApplicationLogon _appLogon;
		private IMultiTenancyWindowsLogon _winLogon;
		private IMessageBrokerComposite _mBroker;
		private ISharedSettingsQuerier _sharedSettingsQuerier;
		private IRepositoryFactory _repFactory;

		[SetUp]
		public void Setup()
		{
			_view = MockRepository.GenerateMock<ILogonView>();
			_model = new LogonModel();
			_initializer = MockRepository.GenerateMock<ILoginInitializer>();
			_logOnOff = MockRepository.GenerateMock<ILogOnOff>();
			_appLogon = MockRepository.GenerateMock<IMultiTenancyApplicationLogon>();
			_winLogon = MockRepository.GenerateMock<IMultiTenancyWindowsLogon>();
			_mBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			_sharedSettingsQuerier = MockRepository.GenerateMock<ISharedSettingsQuerier>();
			_repFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_target = new MultiTenancyLogonPresenter(_view, _model, _initializer,  _logOnOff,
				_mBroker, _appLogon, _winLogon, _sharedSettingsQuerier, _repFactory);
			_model.AuthenticationType = AuthenticationTypeOption.Application;
		}

		[Test]
		public void ShouldCallStartLogonOnViewAtStartUp()
		{
			_view.Stub(x => x.StartLogon(_mBroker)).Return(true);
			_target.Start();
		}


		[Test]
		public void ShouldNotReloadSdkOnBackFromDataSources()
		{
			_model.SelectedDataSourceContainer = new DataSourceContainer(null, AuthenticationTypeOption.Application);
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


		[Test, Ignore("Ola: temp")]
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
			//_appLogon.Stub(x => x.Logon(_model,  MultiTenancyLogonPresenter.UserAgent)).Return(new AuthenticationResult { Successful = true }).IgnoreArguments();

			dataSourceContainer.Stub(x => x.User).Return(person);
			person.Stub(x => x.ApplicationAuthenticationInfo).Return(appAuthInfo);
			appAuthInfo.Stub(x => x.Password = "PASS");
			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.AvailableBusinessUnits(_repFactory)).Return(new List<IBusinessUnit>());
			_view.Stub(x => x.ShowErrorMessage(Resources.NoAllowedBusinessUnitFoundInCurrentDatabase, Resources.ErrorMessage));
			_view.Stub(x => x.ShowStep(true));
			_target.CurrentStep = LoginStep.Login;
			_target.OkbuttonClicked();
		}

		[Test, Ignore("Ola: temp")]
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
			//_appLogon.Stub(x => x.Logon(_model, MultiTenancyLogonPresenter.UserAgent)).Return(new AuthenticationResult { Successful = true }).IgnoreArguments();
			dataSourceContainer.Stub(x => x.User).Return(person);
			person.Stub(x => x.ApplicationAuthenticationInfo).Return(appAuthInfo);
			appAuthInfo.Stub(x => x.Password = "PASS");
			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.AvailableBusinessUnits(_repFactory)).Return(new List<IBusinessUnit> { bu });
			_view.Stub(x => x.ClearForm(Resources.InitializingTreeDots));
			_target.CurrentStep = LoginStep.Login;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldGoToLoginIfApplication()
		{
			var dataSourceContainer = new DataSourceContainer(null, AuthenticationTypeOption.Application);
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
			var uowFact = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var person = MockRepository.GenerateMock<IPerson>();
			var buProvider = MockRepository.GenerateMock<IAvailableBusinessUnitsProvider>();
			var bu = new BusinessUnit("Bu One");
			_model.SelectedDataSourceContainer = dataSourceContainer;
			_model.SelectedBu = bu;

			_view.Stub(x => x.ClearForm(Resources.InitializingTreeDots));
			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.LoadHierarchyInformation(bu, _repFactory)).Return(bu);
			dataSourceContainer.Stub(x => x.User).Return(person);
			_logOnOff.Stub(x => x.LogOn(dataSource, person, bu));
			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(uowFact);
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
			buProvider.Stub(x => x.LoadHierarchyInformation(bu, _repFactory)).Return(bu);
			dataSourceContainer.Stub(x => x.User).Return(person);
			_logOnOff.Stub(x => x.LogOn(dataSource, person, bu));
			dataSourceContainer.Stub(x => x.DataSource).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(uowFact);
			_initializer.Stub(x => x.InitializeApplication(dataSourceContainer)).Return(false);
			_view.Stub(x => x.Exit(DialogResult.Cancel));

			_target.CurrentStep = LoginStep.SelectBu;
			_target.OkbuttonClicked();
		}

		[Test, Ignore("Ola: temp")]
		public void ShouldGetBusAfterDataSourcesIfWindows()
		{
			var dataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();
			var buProvider = MockRepository.GenerateMock<IAvailableBusinessUnitsProvider>();
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			var bu = new BusinessUnit("Bu One");
			var bu2 = new BusinessUnit("Bu two");
			_model.SelectedDataSourceContainer = dataSourceContainer;

			dataSourceContainer.Stub(x => x.AvailableBusinessUnitProvider).Return(buProvider);
			buProvider.Stub(x => x.AvailableBusinessUnits(_repFactory)).Return(new List<IBusinessUnit> { bu, bu2 });
			_view.Stub(x => x.ShowStep(true));

			_target.CurrentStep = LoginStep.SelectLogonType;
			_target.OkbuttonClicked();
		}

		[Test, Ignore("Ola: temp")]
		public void ShouldGoToAppLoginIfWindowsFails()
		{
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			_view.Stub(x => x.ShowStep(true));

			_target.CurrentStep = LoginStep.SelectLogonType;
			_target.OkbuttonClicked();
			_model.AuthenticationType.Should().Be.EqualTo(AuthenticationTypeOption.Application);
			_model.Warning.Should().Be.EqualTo("ajajaj");
			_target.CurrentStep.Should().Be.EqualTo(LoginStep.Login);
		}

		[Test, Ignore("Ola: temp")]
		public void ShouldGoToDatasourceStepIfWebException()
		{
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			_view.Stub(x => x.ShowStep(false));

			_target.CurrentStep = LoginStep.SelectLogonType;
			_target.OkbuttonClicked();
			_target.CurrentStep.Should().Be.EqualTo(LoginStep.SelectLogonType);
		}

		[Test]
		public void ShouldGoDirectToApplicationIfWindowsNotPossible()
		{
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			_winLogon.Stub( x => x.CheckWindowsIsPossible()).Return(false);

			_target.Initialize();
			_target.CurrentStep.Should().Be(LoginStep.Login);
			_model.AuthenticationType.Should().Be.EqualTo(AuthenticationTypeOption.Application);
		}
	}
}
