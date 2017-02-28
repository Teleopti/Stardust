﻿using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Main
{
	public class LogonPresenter : ILogonPresenter
	{
		private readonly ILogonView _view;
		private readonly LogonModel _model;
		private readonly ILoginInitializer _initializer;
		private readonly ILogOnOff _logOnOff;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly ISharedSettingsQuerier _sharedSettingsQuerier;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;
		private readonly IAvailableBusinessUnitsProvider _availableBusinessUnitsProvider;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LogonPresenter));
		private readonly ILog _customLogger = LogManager.GetLogger("CustomEOLogger");

		public LogonPresenter(ILogonView view, LogonModel model, ILoginInitializer initializer, ILogOnOff logOnOff,
			IMessageBrokerComposite messageBroker, ISharedSettingsQuerier sharedSettingsQuerier,
			IAuthenticationQuerier authenticationQuerier, IWindowsUserProvider windowsUserProvider,
			IAvailableBusinessUnitsProvider availableBusinessUnitsProvider)
		{
			_view = view;
			_model = model;
			_initializer = initializer;
			_logOnOff = logOnOff;
			_messageBroker = messageBroker;
			_sharedSettingsQuerier = sharedSettingsQuerier;
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
			_availableBusinessUnitsProvider = availableBusinessUnitsProvider;
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
		}

		public LoginStep CurrentStep { get; set; }

		private void logInfo(string message)
		{
			_customLogger.Info("LogonPresenter: " + message);
		}

		public bool Start(string serverUrl)
		{
			CurrentStep = LoginStep.SelectLogonType;
			_view.ServerUrl = serverUrl;
			logInfo("EO Browser: show the login screen.");
			return _view.StartLogon();
			
		}

		public void Initialize()
		{
			getLogonType();
		}

		public void OkbuttonClicked()
		{
			if (checkModel())
			{
				CurrentStep++;
				if (CurrentStep == LoginStep.Login && _model.AuthenticationType == AuthenticationTypeOption.Windows)
					CurrentStep++;
			}
			dataForCurrentStep();
		}

		private bool checkModel()
		{
			switch (CurrentStep)
			{
				case LoginStep.Login:
					return _model.HasValidLogin();
				case LoginStep.SelectBu:
					return _model.SelectedBu != null;
			}
			return true;
		}

		public void BackButtonClicked()
		{
			CurrentStep--;
			dataForCurrentStep();
		}

		private void dataForCurrentStep()
		{
			switch (CurrentStep)
			{
				case LoginStep.SelectLogonType:
					getLogonType();
					break;
				case LoginStep.Login:
					_view.ShowStep(true);
					break;
				case LoginStep.SelectBu:
					getBusinessUnits();
					break;
				case LoginStep.Loading:
					initApplication();
					break;
			}
		}

		private void getLogonType()
		{
			if (!StateHolderReader.IsInitialized)
			{
				var settings = _sharedSettingsQuerier.GetSharedSettings();
				_view.InitStateHolderWithoutDataSource(_messageBroker, settings);
			}
			if (!_authenticationQuerier.TryLogon(new IdentityLogonClientModel {Identity = _windowsUserProvider.Identity()}, string.Empty).Success)
			{
				_model.AuthenticationType = AuthenticationTypeOption.Application;
				CurrentStep++;
			}
			_view.ShowStep(false); //once a sdk is loaded it is not changeable
		}

		private void getBusinessUnits()
		{
			try
			{
				if (_model.AuthenticationType == AuthenticationTypeOption.Application)
				{
					if (!login())
					{
						CurrentStep--;
						return;
					}
				}
				if (_model.AuthenticationType == AuthenticationTypeOption.Windows)
				{
					if (!winLogin())
					{
						CurrentStep--;
						_view.ShowStep(true);
						return;
					}
				}
				
			}
			catch (WebException exception)
			{
				var message = exception.Message;
				if (exception.InnerException != null)
					message = message + " " + exception.InnerException.Message;
				_view.ShowErrorMessage(message,"Logon Error");
				CurrentStep = LoginStep.SelectLogonType;
				_view.ShowStep(false);
				
				return;
			}

			_model.AvailableBus = _availableBusinessUnitsProvider.AvailableBusinessUnits(_model.SelectedDataSourceContainer.User, _model.SelectedDataSourceContainer.DataSource).ToList();
			if (_model.AvailableBus.Count == 0)
			{
				_view.ShowErrorMessage(Resources.NoAllowedBusinessUnitFoundInCurrentDatabase, Resources.ErrorMessage);
				CurrentStep--;
			}

			if (_model.AvailableBus.Count == 1)
			{
				_model.SelectedBu = _model.AvailableBus[0];
				initApplication();
				return;
			}
			_view.ShowStep(true);
		}

		private bool login()
		{
			if (!StateHolderReader.IsInitialized)
			{
				var settings = _sharedSettingsQuerier.GetSharedSettings();
				_view.InitStateHolderWithoutDataSource(_messageBroker, settings);
			}
			var authenticationResult = _authenticationQuerier.TryLogon(new ApplicationLogonClientModel { UserName = _model.UserName, Password = _model.Password }, UserAgentConstant.UserAgentWin);
				
			if (authenticationResult.Success)
			{
				_model.SelectedDataSourceContainer = new DataSourceContainer(authenticationResult.DataSource, authenticationResult.Person);
				WinTenantCredentials.SetCredentials(authenticationResult.Person.Id.Value, authenticationResult.TenantPassword);
				return true;
			}

			_view.ShowErrorMessage(string.Concat(authenticationResult.FailReason, "  "), Resources.ErrorMessage);
			return false;
		}

		private bool winLogin()
		{
			var authenticationResult = _authenticationQuerier.TryLogon(new IdentityLogonClientModel { Identity = _windowsUserProvider.Identity() }, UserAgentConstant.UserAgentWin);

			if (authenticationResult.Success)
			{
				_model.SelectedDataSourceContainer = new DataSourceContainer(authenticationResult.DataSource, authenticationResult.Person);
				WinTenantCredentials.SetCredentials(authenticationResult.Person.Id.Value, authenticationResult.TenantPassword);
				return true;
			}
			_model.Warning = authenticationResult.FailReason;
			// windows does not work we need to use application
			_model.AuthenticationType = AuthenticationTypeOption.Application;
			return false;
		}

		public bool webLogin(Guid businessunitId)
		{
			logInfo("EO Browser: Authenticating the user in web");
			try
		    {
                var authenticationResult = _authenticationQuerier.TryLogon(new IdLogonClientModel { Id = _model.PersonId }, UserAgentConstant.UserAgentWin);
                if (!StateHolderReader.IsInitialized)
                {
                    var settings = _sharedSettingsQuerier.GetSharedSettings();
                    _view.InitStateHolderWithoutDataSource(_messageBroker, settings);
                }
                if (authenticationResult.Success)
                {
					logInfo("EO Browser: Authentication was successful");
						  _model.SelectedDataSourceContainer = new DataSourceContainer(authenticationResult.DataSource, authenticationResult.Person);
                    WinTenantCredentials.SetCredentials(authenticationResult.Person.Id.Value, authenticationResult.TenantPassword);
                    using (var uow = _model.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
                    {
                        var businessUnit = new BusinessUnitRepository(uow).Load(businessunitId);
                        _model.SelectedBu = businessUnit;

                    }
                    initApplication();
                    return true;
                }
				logInfo("EO Browser: Authentication was unsuccessful");
				_model.Warning = authenticationResult.FailReason;
                _model.AuthenticationType = AuthenticationTypeOption.Application;
                return false;
            }
		    catch (Exception e)
		    {
                Logger.Error(e.Message);
                _model.Warning = e.Message;
                MessageBox.Show(e.Message,Resources.ErrorOccuredWhenAccessingTheDataSource);
                _view.Exit(DialogResult.Cancel);
                return false;
		    }
			
		}
		private void initApplication()
		{
			logInfo("EO Browser: Loading the main application");
			_view.ClearForm(Resources.InitializingTreeDots);
			setBusinessUnit();
			if (!_initializer.InitializeApplication(_model.SelectedDataSourceContainer))
			{
				_view.Exit(DialogResult.Cancel);
				return;
			}
			logInfo("EO Browser: Loaded the main application disposing the login screen now.");
			_view.Exit(DialogResult.OK);
		}

		private void setBusinessUnit()
		{
			Logger.Info("EO Browser: Setting business unit before loading fat client");
			var businessUnit = _model.SelectedBu;
			businessUnit = _availableBusinessUnitsProvider.LoadHierarchyInformation(_model.SelectedDataSourceContainer.DataSource, businessUnit);

			_logOnOff.LogOn(_model.SelectedDataSourceContainer.DataSource, _model.SelectedDataSourceContainer.User, businessUnit);

			AuthenticationTypeOption = _model.AuthenticationType;
		}

		//moved from ISessionData. Should be some object instead of static data, but just keep same behavior
		public static AuthenticationTypeOption AuthenticationTypeOption { get; private set; }
	}
}
