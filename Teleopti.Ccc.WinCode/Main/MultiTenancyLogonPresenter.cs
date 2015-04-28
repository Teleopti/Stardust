using System.Linq;
using System.Net;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Web;
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
		bool Start();
		LoginStep CurrentStep { get; set; }
	}

	public enum LoginStep
	{
		SelectLogonType = 0,
		Login = 1,
		SelectBu = 2,
		Loading = 3
	}

	public class MultiTenancyLogonPresenter : ILogonPresenter
	{
		private readonly ILogonView _view;
		private readonly LogonModel _model;
		private readonly ILoginInitializer _initializer;
		private readonly ILogOnOff _logOnOff;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IMultiTenancyApplicationLogon _applicationLogon;
		private readonly IMultiTenancyWindowsLogon _multiTenancyWindowsLogon;
		private readonly ISharedSettingsQuerier _sharedSettingsQuerier;
		public const string UserAgent = "WIN";


		public MultiTenancyLogonPresenter(ILogonView view, LogonModel model,
			ILoginInitializer initializer,
			ILogOnOff logOnOff,
			IMessageBrokerComposite messageBroker,
			IMultiTenancyApplicationLogon applicationLogon,
			IMultiTenancyWindowsLogon multiTenancyWindowsLogon,
			ISharedSettingsQuerier sharedSettingsQuerier
			)
		{
			_view = view;
			_model = model;
			_initializer = initializer;
			_logOnOff = logOnOff;
			_messageBroker = messageBroker;
			_applicationLogon = applicationLogon;
			_multiTenancyWindowsLogon = multiTenancyWindowsLogon;
			_sharedSettingsQuerier = sharedSettingsQuerier;
			_model.AuthenticationType = AuthenticationTypeOption.Windows;
		}

		public LoginStep CurrentStep { get; set; }

		public bool Start()
		{
			CurrentStep = LoginStep.SelectLogonType;
			return _view.StartLogon(_messageBroker);
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
			if (CurrentStep == LoginStep.Login &&
				 _model.SelectedDataSourceContainer.AuthenticationTypeOption == AuthenticationTypeOption.Windows)
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
				if (!_view.InitStateHolderWithoutDataSource(_messageBroker, settings))
					CurrentStep--; //?
			}
			if (!_multiTenancyWindowsLogon.CheckWindowsIsPossible())
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
			var authenticationResult = _applicationLogon.Logon(_model, StateHolderReader.Instance.StateReader.ApplicationScopeData, UserAgent);

			if (authenticationResult.HasMessage)
				_view.ShowErrorMessage(string.Concat(authenticationResult.Message, "  "), Resources.ErrorMessage);

			return authenticationResult.Successful;
		}

		private bool winLogin()
		{
			//var authenticationResult = _multiTenancyWindowsLogon.Logon(_model, UserAgent);
			var authenticationResult = _multiTenancyWindowsLogon.Logon(_model, StateHolderReader.Instance.StateReader.ApplicationScopeData, UserAgent);

			if (authenticationResult.HasMessage)
				_model.Warning = authenticationResult.Message;
				//_view.ShowErrorMessage(string.Concat(authenticationResult.Message, "  "), Resources.ErrorMessage);

			if (authenticationResult.Successful)
			{
				//if (!_view.InitStateHolder(_messageBroker, authenticationResult.PasswordPolicy))
					//return false;
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
			StateHolderReader.Instance.StateReader.SessionScopeData.UserName = _model.UserName;
		}
	}
}
