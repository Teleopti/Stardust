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
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
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
		private IMessageBrokerComposite _mBroker;
		private ISharedSettingsQuerier _sharedSettingsQuerier;
		private IRepositoryFactory _repFactory;
		private IAuthenticationQuerier _authenticationQuerier;

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
			_repFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_target = new MultiTenancyLogonPresenter(_view, _model, _initializer,  _logOnOff,
				_mBroker, _sharedSettingsQuerier, _repFactory, _authenticationQuerier, new EnvironmentWindowsUserProvider());
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
			_model.SelectedDataSourceContainer = new DataSourceContainer(null);
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
			var uowFact = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var person = MockRepository.GenerateMock<IPerson>();
			var permiss = MockRepository.GenerateMock<IPermissionInformation>();
			var appAuthInfo = new AuthenticationResult{Successful = true, Person = person, DataSource = dataSource};
			var buRep = MockRepository.GenerateMock<IBusinessUnitRepository>();
			
			_model.UserName = "USER";
			_model.Password = "PASS";

			_appLogon.Stub(x => x.Logon("USER","PASS", "WIN")).Return(appAuthInfo);
			
			person.Stub(x => x.PermissionInformation).Return(permiss);
			permiss.Stub(x => x.HasAccessToAllBusinessUnits()).Return(true);
			dataSource.Stub(x => x.Application).Return(uowFact);
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			uowFact.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_repFactory.Stub(x => x.CreateBusinessUnitRepository(uow)).Return(buRep);
			buRep.Stub(x => x.LoadAllBusinessUnitSortedByName()).Return(new List<IBusinessUnit>());
			
			_view.Stub(x => x.ShowStep(true));
			_target.CurrentStep = LoginStep.Login;
			_target.OkbuttonClicked();
			_view.AssertWasCalled(x => x.ShowErrorMessage(Resources.NoAllowedBusinessUnitFoundInCurrentDatabase, Resources.ErrorMessage));
		}

		[Test]
		public void ShouldGoToLoginIfApplication()
		{
			var dataSourceContainer = new DataSourceContainer(null);
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

		[Test]
		public void ShouldGetBusAfterDataSourcesIfWindows()
		{
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			var bu = new BusinessUnit("Bu One");
			var bu2 = new BusinessUnit("Bu two");
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowFact = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var person = MockRepository.GenerateMock<IPerson>();
			var permiss = MockRepository.GenerateMock<IPermissionInformation>();
			var appAuthInfo = new AuthenticationResult { Successful = true, Person = person, DataSource = dataSource };
			var buRep = MockRepository.GenerateMock<IBusinessUnitRepository>();
			
			_winLogon.Stub(x => x.Logon("WIN")).Return(appAuthInfo);

			person.Stub(x => x.PermissionInformation).Return(permiss);
			permiss.Stub(x => x.HasAccessToAllBusinessUnits()).Return(true);
			dataSource.Stub(x => x.Application).Return(uowFact);
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			uowFact.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_repFactory.Stub(x => x.CreateBusinessUnitRepository(uow)).Return(buRep);
			buRep.Stub(x => x.LoadAllBusinessUnitSortedByName()).Return(new List<IBusinessUnit> { bu, bu2 });

			_view.Stub(x => x.ShowStep(true));

			_target.CurrentStep = LoginStep.SelectLogonType;
			_target.OkbuttonClicked();
		}

		[Test]
		public void ShouldGoDirectToApplicationIfWindowsNotPossible()
		{
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
			_authenticationQuerier.Stub(x => x.TryLogon(new IdentityLogonClientModel(), null)).IgnoreArguments().Return(new AuthenticationQuerierResult{Success = false});

			_target.Initialize();
			_target.CurrentStep.Should().Be(LoginStep.Login);
			_model.AuthenticationType.Should().Be.EqualTo(AuthenticationTypeOption.Application);
		}
	}
}
