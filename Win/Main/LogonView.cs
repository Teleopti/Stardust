using System;
using System.Collections.Generic;
using System.Configuration;
using System.Deployment.Application;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Main.LogonScreens;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LogonView : Form, ILogonView
	{
		public LogonPresenter Presenter
		{
			get;
			set;
			//get
			//{
			//	return Presenter ?? new LogonPresenter(this, new LogonModel());
			//}
		}

		public LoginStep CurrentStep { get; private set; }

		private readonly IList<ILogonStep> _logonSteps;
		private readonly InitializingScreen _initializingScreen;
		private readonly SelectSdkScreen _selectSdkScreen;
		private readonly SelectDatasourceScreen _selectDatasourceScreen;
		private readonly LoginScreen _loginScreen;
		private readonly LoadingScreen _loadingScreen;

		private readonly bool getConfigFromWebService;

		public LogonView()
		{
			InitializeComponent();

			_initializingScreen = new InitializingScreen(this);
			_selectSdkScreen = new SelectSdkScreen(this);
			_selectDatasourceScreen = new SelectDatasourceScreen(this);
			_loginScreen = new LoginScreen(this);
			_loadingScreen = new LoadingScreen(this);

			_logonSteps = new List<ILogonStep>
				{
					_initializingScreen,
					_selectSdkScreen,
					_selectDatasourceScreen,
					_loginScreen,
					_loadingScreen
				};

			CurrentStep = LoginStep.Initializing;
			getConfigFromWebService = Convert.ToBoolean(ConfigurationManager.AppSettings["GetConfigFromWebService"],
			                                            CultureInfo.InvariantCulture);
		}

		public void StartLogon()
		{
			var endpoints = ServerEndpointSelector.GetEndpointNames();
			if (!getConfigFromWebService && endpoints.Count(s => !string.IsNullOrEmpty(s)) <= 1)
			{
				StepForward();
				StepForward();
				return;
			}
			StepForward();
		}

		public void OkButtonClicked(object data)
		{
			Presenter.OkbuttonClicked(data);
		}

		public void CancelButtonClicked()
		{
			DialogResult = DialogResult.Cancel;
		}

		public void BackButtonClicked()
		{
			Presenter.BackButtonClicked();
		}

		public void StepForward()
		{
			CurrentStep++;
			refreshView();
		}

		public void StepBackwards()
		{
			CurrentStep--;
			refreshView();
		}

		private void refreshView()
		{
			ShowInTaskbar = (int)CurrentStep != 0;
			var currentControl = _logonSteps[(int) CurrentStep];
			updatePanel((UserControl) currentControl);
			currentControl.SetData(Presenter.GetDataForCurrentStep());
			Refresh();
		}

		private void updatePanel(Control userControl)
		{
			pnlContent.Controls.Clear();
			pnlContent.Controls.Add(userControl);
		}
		
		public void Exit()
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
