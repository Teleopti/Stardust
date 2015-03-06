using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonPresenter
	{
		void OkbuttonClicked();
		void BackButtonClicked();
		void Initialize();
		void GetDataForCurrentStep(bool goingBack);
		bool Start();
		LoginStep CurrentStep { get; set; }
	}

	public enum LoginStep
	{
		SelectSdk = 0,
		SelectLogonType = 1,
		Login = 2,
		SelectBu = 3,
		Loading = 4, // not used
		Ready = 5 // not used
	}

	public class MultiTenancyLogonPresenter : ILogonPresenter
	{
		private readonly ILogonView _view;
		private readonly ILogonModel _model;
		private readonly ILoginInitializer _initializer;
		private readonly ILogOnOff _logOnOff;
		private readonly IServerEndpointSelector _serverEndpointSelector;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IMultiTenancyApplicationLogon _applicationLogon;
		private readonly IMultiTenancyWindowsLogon _multiTenancyWindowsLogon;
		public const string UserAgent = "WIN";


		public MultiTenancyLogonPresenter(ILogonView view, LogonModel model,
			ILoginInitializer initializer,
			ILogOnOff logOnOff,
			IServerEndpointSelector serverEndpointSelector,
			IMessageBrokerComposite messageBroker,
			IMultiTenancyApplicationLogon applicationLogon,
			IMultiTenancyWindowsLogon multiTenancyWindowsLogon
			)
		{
			_view = view;
			_model = model;
			_initializer = initializer;
			_logOnOff = logOnOff;
			_serverEndpointSelector = serverEndpointSelector;
			_messageBroker = messageBroker;
			_applicationLogon = applicationLogon;
			_multiTenancyWindowsLogon = multiTenancyWindowsLogon;
			if (ConfigurationManager.AppSettings["GetConfigFromWebService"] != null)
				_model.GetConfigFromWebService = Convert.ToBoolean(ConfigurationManager.AppSettings["GetConfigFromWebService"],
					CultureInfo.InvariantCulture);
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
		}

		public LoginStep CurrentStep { get; set; }

		public bool Start()
		{
			CurrentStep = LoginStep.SelectSdk;
			return _view.StartLogon(_messageBroker);
		}

		public void Initialize()
		{
			getSdks();
		}

		public void OkbuttonClicked()
		{
			if (checkModel())
			{
				CurrentStep++;
				if (CurrentStep == LoginStep.Login && _model.AuthenticationType == AuthenticationTypeOption.Windows)
					CurrentStep++;
			}
			GetDataForCurrentStep(false);
		}

		private bool checkModel()
		{
			switch (CurrentStep)
			{
				case LoginStep.SelectSdk:
					return _model.SelectedSdk != null;
				case LoginStep.SelectLogonType:
					return checkAndReportDataSources();
				case LoginStep.Login:
					return _model.HasValidLogin();
				case LoginStep.SelectBu:
					return _model.SelectedBu != null;
			}
			return true;
		}

		private bool checkAndReportDataSources()
		{
			return true;
		}

		public void BackButtonClicked()
		{
			CurrentStep--;
			if (CurrentStep == LoginStep.Login &&
				 _model.SelectedDataSourceContainer.AuthenticationTypeOption == AuthenticationTypeOption.Windows)
				CurrentStep--;

			GetDataForCurrentStep(true);
		}

		public void GetDataForCurrentStep(bool goingBack)
		{
			switch (CurrentStep)
			{
				case LoginStep.SelectSdk:
					if (!goingBack)
						getSdks();
					break;
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

		private void getSdks()
		{
			_view.ClearForm("");
			var endpoints = _serverEndpointSelector.GetEndpointNames();
			_model.Sdks = endpoints;
			if (endpoints.Count == 1)
			{
				_model.SelectedSdk = endpoints[0];
				CurrentStep++;
				getLogonType();
			}


			_view.ShowStep(false);
		}

		private void getLogonType()
		{
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
			
			var provider = _model.SelectedDataSourceContainer.AvailableBusinessUnitProvider;
			_model.AvailableBus = provider.AvailableBusinessUnits().ToList();
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
			var authenticationResult = _applicationLogon.Logon(_model, UserAgent);
			var choosenDataSource = _model.SelectedDataSourceContainer;

			if (authenticationResult.HasMessage)
				_view.ShowErrorMessage(string.Concat(authenticationResult.Message, "  "), Resources.ErrorMessage);

			if (authenticationResult.Successful)
			{
				//To use for silent background log on
				choosenDataSource.User.ApplicationAuthenticationInfo.Password = _model.Password;
				if (!_view.InitStateHolder(_messageBroker, authenticationResult.PasswordPolicy))
					return false;
				return true;
			}

			return false;
		}

		private bool winLogin()
		{
			var authenticationResult = _multiTenancyWindowsLogon.Logon(_model, StateHolderReader.Instance.StateReader.ApplicationScopeData, UserAgent);

			if (authenticationResult.HasMessage)
				_model.Warning = authenticationResult.Message;
				//_view.ShowErrorMessage(string.Concat(authenticationResult.Message, "  "), Resources.ErrorMessage);

			if (authenticationResult.Successful)
			{
				if (!_view.InitStateHolder(_messageBroker, authenticationResult.PasswordPolicy))
					return false;
				return true;
			}
			// windows does not work we need to use application
			_model.AuthenticationType = AuthenticationTypeOption.Application;
			return false;
		}

		private void initApplication()
		{
			_view.ClearForm(Resources.InitializingTreeDots);
			setBusinessUnit();
			if (!_initializer.InitializeApplication(_model.SelectedDataSourceContainer))
			{
				_view.Exit(DialogResult.Cancel);
				return;
			}
			_view.Exit(DialogResult.OK);
		}

		private void setBusinessUnit()
		{
			var businessUnit = _model.SelectedBu;
			businessUnit = _model.SelectedDataSourceContainer.AvailableBusinessUnitProvider.LoadHierarchyInformation(businessUnit);

			_logOnOff.LogOn(_model.SelectedDataSourceContainer.DataSource, _model.SelectedDataSourceContainer.User, businessUnit);

			StateHolderReader.Instance.StateReader.SessionScopeData.AuthenticationTypeOption = _model.AuthenticationType;
		}
	}
}
