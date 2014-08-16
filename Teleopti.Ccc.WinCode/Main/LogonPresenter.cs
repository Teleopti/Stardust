using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonPresenter
	{
		void OkbuttonClicked();
		void BackButtonClicked();
		void Initialize();
		void GetDataForCurrentStep();
		bool Start();
		LoginStep CurrentStep { get; set; }
	}

	public class LogonPresenter : ILogonPresenter
	{
		private readonly ILogonView _view;
		private readonly LogonModel _model;
		private readonly ILoginInitializer _initializer;
		private readonly ILogonLogger _logonLogger;
		private readonly ILogOnOff _logOnOff;
		private readonly IServerEndpointSelector _serverEndpointSelector;
		private readonly IDataSourceHandler _dataSourceHandler;

		public LogonPresenter(ILogonView view, LogonModel model,
		                      IDataSourceHandler dataSourceHandler, ILoginInitializer initializer,
		                      ILogonLogger logonLogger, ILogOnOff logOnOff,
		                      IServerEndpointSelector serverEndpointSelector)
		{
			_view = view;
			_model = model;
			_dataSourceHandler = dataSourceHandler;
			_initializer = initializer;
			_logonLogger = logonLogger;
			_logOnOff = logOnOff;
			_serverEndpointSelector = serverEndpointSelector;
			if (ConfigurationManager.AppSettings["GetConfigFromWebService"] != null)
				_model.GetConfigFromWebService = Convert.ToBoolean(ConfigurationManager.AppSettings["GetConfigFromWebService"],
				                                                   CultureInfo.InvariantCulture);
		}

		public LoginStep CurrentStep { get; set; }

		public bool Start()
		{
			CurrentStep = LoginStep.SelectSdk;
			return _view.StartLogon();
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
				if (CurrentStep == LoginStep.Login &&
				    _model.SelectedDataSourceContainer.AuthenticationTypeOption == AuthenticationTypeOption.Windows)
					CurrentStep++;
			}
			GetDataForCurrentStep();
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
			if (_model.SelectedDataSourceContainer == null || !_model.DataSourceContainers.Any())
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

			GetDataForCurrentStep();
		}

		public void GetDataForCurrentStep()
		{
			switch (CurrentStep)
			{
				case LoginStep.SelectSdk:
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
				_view.InitializeAndCheckStateHolder(_model.SelectedSdk);
				var logonableDataSources = new List<IDataSourceContainer>();
				foreach (var dataSourceProvider in _dataSourceHandler.DataSourceProviders())
				{
					logonableDataSources.AddRange(dataSourceProvider.DataSourceList());
				}
				_model.DataSourceContainers = logonableDataSources;
			}
			if (_model.DataSourceContainers.Count == 1)
			{
				_model.SelectedDataSourceContainer = _model.DataSourceContainers.Single();
				CurrentStep++;
				GetDataForCurrentStep();
			}
			_view.ShowStep(_model.Sdks.Count > 1);
		}

		private void getBusinessUnits()
		{
			if (_model.SelectedDataSourceContainer.AuthenticationTypeOption == AuthenticationTypeOption.Application &&
				!login())
			{
				CurrentStep--;
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
			var choosenDataSource = _model.SelectedDataSourceContainer;
			var authenticationResult = choosenDataSource.LogOn(_model.UserName, _model.Password);

			if (authenticationResult.HasMessage)
				_view.ShowErrorMessage(string.Concat(authenticationResult.Message, "  "), Resources.ErrorMessage);

			if (authenticationResult.Successful)
			{
				//To use for silent background log on
				choosenDataSource.User.ApplicationAuthenticationInfo.Password = _model.Password;
				return true;
			}
			var model = new LoginAttemptModel
				{
					ClientIp = ipAdress(),
					Client = "WIN",
					UserCredentials = choosenDataSource.LogOnName,
					Provider = choosenDataSource.AuthenticationTypeOption.ToString(),
					Result = "LogonFailed"
				};

			_logonLogger.SaveLogonAttempt(model, choosenDataSource.DataSource.Application);

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

			var model = new LoginAttemptModel
				{
					ClientIp = ipAdress(),
					Client = "WIN",
					UserCredentials = _model.SelectedDataSourceContainer.LogOnName,
					Provider = _model.SelectedDataSourceContainer.AuthenticationTypeOption.ToString(),
					Result = "LogonSuccess"
				};
			if (_model.SelectedDataSourceContainer.User != null) model.PersonId = _model.SelectedDataSourceContainer.User.Id;

			_logonLogger.SaveLogonAttempt(model, _model.SelectedDataSourceContainer.DataSource.Application);

			StateHolderReader.Instance.StateReader.SessionScopeData.AuthenticationTypeOption =
				_model.SelectedDataSourceContainer.AuthenticationTypeOption;
		}

		private static string ipAdress()
		{
			var ips = Dns.GetHostEntry(Dns.GetHostName());
			var ip = "";
			foreach (var adress in ips.AddressList.Where(adress => adress.AddressFamily == AddressFamily.InterNetwork))
				ip = adress.ToString();
			return ip;
		}
	}

	public enum LoginStep
	{
		SelectSdk = 0,
		SelectDatasource = 1,
		Login = 2,
		SelectBu = 3,
		Loading = 4, // not used
		Ready = 5 // not used
	}
}
