using System;
using System.Windows.Forms;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main
{
	public class LogonPresenter
	{
		private readonly ILogonView _view;
		private readonly LogonModel _model;
		private readonly ILoginInitializer _initializer;
		private readonly ILogOnOff _logOnOff;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly ISharedSettingsTenantClient _sharedSettingsQuerier;
		private readonly IAuthenticationTenantClient _authenticationQuerier;
		private readonly IAvailableBusinessUnitsProvider _availableBusinessUnitsProvider;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(LogonPresenter));
		private readonly ILog _customLogger = LogManager.GetLogger("CustomEOLogger");
		private readonly IApplicationInsights _applicationInsights;

		public LogonPresenter(ILogonView view, LogonModel model, ILoginInitializer initializer, ILogOnOff logOnOff,
			IMessageBrokerComposite messageBroker, ISharedSettingsTenantClient sharedSettingsQuerier,
			IAuthenticationTenantClient authenticationQuerier, IAvailableBusinessUnitsProvider availableBusinessUnitsProvider, 
			IApplicationInsights applicationInsights)
		{
			_view = view;
			_model = model;
			_initializer = initializer;
			_logOnOff = logOnOff;
			_messageBroker = messageBroker;
			_sharedSettingsQuerier = sharedSettingsQuerier;
			_authenticationQuerier = authenticationQuerier;
			_availableBusinessUnitsProvider = availableBusinessUnitsProvider;
			_applicationInsights = applicationInsights;
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
		}

		private void logInfo(string message)
		{
			_customLogger.Info("LogonPresenter: " + message);
		}

		public bool Start(string serverUrl)
		{
			_applicationInsights.Init();
			_view.ServerUrl = serverUrl;
			logInfo("EO Browser: show the login screen.");
			return _view.StartLogon();

		}

		public bool WebLogin(Guid businessunitId)
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
				MessageBox.Show(e.Message, Resources.ErrorOccuredWhenAccessingTheDataSource);
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

			_logOnOff.LogOnWithoutPermissions(_model.SelectedDataSourceContainer.DataSource, _model.SelectedDataSourceContainer.User, businessUnit);

			AuthenticationTypeOption = _model.AuthenticationType;
		}

		//moved from ISessionData. Should be some object instead of static data, but just keep same behavior
		public static AuthenticationTypeOption AuthenticationTypeOption { get; private set; }
	}
}
