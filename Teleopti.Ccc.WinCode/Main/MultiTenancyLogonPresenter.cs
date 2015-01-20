using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCode.Main
{
	public class MultiTenancyLogonPresenter : ILogonPresenter
	{
		private readonly ILogonView _view;
		private readonly ILogonModel _model;
		private readonly ILoginInitializer _initializer;
		//private readonly ILogonLogger _logonLogger;
		private readonly ILogOnOff _logOnOff;
		private readonly IServerEndpointSelector _serverEndpointSelector;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IMultiTenancyApplicationLogon _applicationLogon;
		private readonly IMultiTenancyWindowsLogon _multiTenancyWindowsLogon;
		private readonly IDataSourceHandler _dataSourceHandler;

		public MultiTenancyLogonPresenter(ILogonView view, LogonModel model,
			IDataSourceHandler dataSourceHandler, ILoginInitializer initializer,
			//ILogonLogger logonLogger, 
			ILogOnOff logOnOff,
			IServerEndpointSelector serverEndpointSelector,
			IMessageBrokerComposite messageBroker,
			IMultiTenancyApplicationLogon applicationLogon,
			IMultiTenancyWindowsLogon multiTenancyWindowsLogon
			)
		{
			_view = view;
			_model = model;
			_dataSourceHandler = dataSourceHandler;
			_initializer = initializer;
			//_logonLogger = logonLogger;
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
			return _view.StartLogon(false);
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
				case LoginStep.SelectDatasource:
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
			var notAvailableDataSources =
				_dataSourceHandler.AvailableDataSourcesProvider()
				                  .UnavailableDataSources()
				                  .Select(d => d.Application.Name)
				                  .ToList();
			if (notAvailableDataSources.Any())
			{
				var message = notAvailableDataSources.Aggregate("The following data source(s) is currently not available:",
				                                                (current, source) => current + ("\n\t- " + source)) +
				              "\nThe data source server is probably down or the connection string is invalid.";
				_view.ShowWarningMessage(message, Resources.LogOn);
				return false;
			}
			if (!_model.DataSourceContainers.Any())
			{
				_view.ShowErrorMessage(Resources.NoAvailableDataSourcesHasBeenFound, Resources.LogOn);
				return false;
			}

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
					if(!goingBack) 
						getSdks();
					break;
				case LoginStep.SelectDatasource:
					getDataSources();
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
				getDataSources();
			}
			
			
			_view.ShowStep(false);
		}

		private void getDataSources()
		{
			if (_model.DataSourceContainers == null)
			{
				_view.ClearForm(Resources.SearchingForDataSourcesTreeDots);
				_view.InitializeAndCheckStateHolder(_model.SelectedSdk, _messageBroker);
				var logonableDataSources = new List<IDataSourceContainer>();
				foreach (var dataSourceProvider in _dataSourceHandler.DataSourceProviders())
				{
					logonableDataSources.AddRange(dataSourceProvider.DataSourceList());
				}
				_model.DataSourceContainers = logonableDataSources;
			}
			
			_view.ShowStep(false); //once a sdk is loaded it is not changeable
		}

		private void getBusinessUnits()
		{
			if (_model.AuthenticationType == AuthenticationTypeOption.Application)
			{
				if(!login())
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
			var authenticationResult = _applicationLogon.Logon(_model);
			var choosenDataSource = _model.SelectedDataSourceContainer;

			if (authenticationResult.HasMessage)
				_view.ShowErrorMessage(string.Concat(authenticationResult.Message, "  "), Resources.ErrorMessage);

			if (authenticationResult.Successful)
			{
				//To use for silent background log on
				choosenDataSource.User.ApplicationAuthenticationInfo.Password = _model.Password;
				return true;
			}

			return false;
		}

		private bool winLogin()
		{
			var authenticationResult = _multiTenancyWindowsLogon.Logon(_model);
			
			if (authenticationResult.HasMessage)
				_view.ShowErrorMessage(string.Concat(authenticationResult.Message, "  "), Resources.ErrorMessage);

			if (authenticationResult.Successful)
			{
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

			//var model = new LoginAttemptModel
			//	{
			//		ClientIp = ipAdress(),
			//		Client = "WIN",
			//		UserCredentials = _model.UserName,
			//		Provider = _model.AuthenticationType.ToString(),
			//		Result = "LogonSuccess"
			//	};
			//if (_model.SelectedDataSourceContainer.User != null) model.PersonId = _model.SelectedDataSourceContainer.User.Id;

			//_logonLogger.SaveLogonAttempt(model, _model.SelectedDataSourceContainer.DataSource.Application);

			StateHolderReader.Instance.StateReader.SessionScopeData.AuthenticationTypeOption =
				_model.SelectedDataSourceContainer.AuthenticationTypeOption;
		}

		//private static string ipAdress()
		//{
		//	var ips = Dns.GetHostEntry(Dns.GetHostName());
		//	var ip = "";
		//	foreach (var adress in ips.AddressList.Where(adress => adress.AddressFamily == AddressFamily.InterNetwork))
		//		ip = adress.ToString();
		//	return ip;
		//}
	}

	

}
