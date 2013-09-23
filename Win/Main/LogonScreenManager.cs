using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Main.LogonScreens;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LogonScreenManager : Form
	{
		private IList<ILogonStep> _logonSteps;
		private LoginStep _currentStep;

		private InitializingScreen _initializingScreen;
		private SelectSdkScreen _selectSdkScreen;
		private SelectDatasourceScreen _selectDatasourceScreen;
		private LoginScreen _loginScreen;
		private LoadingScreen _loadingScreen;

		public LogonScreenManager()
		{
			InitializeComponent();
			initializeLogic();
		}

		private void initializeLogic()
		{
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

			_currentStep = LoginStep.Initializing;
		}

		public void OkButtonClicked()
		{

		}

		public void CancelButtonClicked()
		{
			DialogResult = DialogResult.Cancel;
		}

		public void BackButtonClicked()
		{

		}

		public void StepForward()
		{
			_currentStep++;
			refreshView();
		}

		public void StepBackwards()
		{
			_currentStep--;
			refreshView();
		}

		private void refreshView()
		{
			ShowInTaskbar = (int) _currentStep != 0;
			updatePanel((UserControl)_logonSteps[(int)_currentStep]);
			Refresh();
		}

		private void updatePanel(Control userControl)
		{
			pnlContent.Controls.Clear();
			pnlContent.Controls.AddRange(userControl.Controls.OfType<Control>().ToArray());
		}

		private enum LoginStep
		{
			Initializing = 0,
			SelectSdk = 1,
			SelectDatasource = 2,
			Login = 3,
			Loading = 4,
			Ready = 5
		}
	}
}
