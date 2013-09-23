using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Main.LogonScreens;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LogonScreenView : Form, ILogonScreenView
	{
		public LoginStep CurrentStep { get; private set; }

		private readonly ILogonScreenPresenter _presenter;
		private IList<ILogonStep> _logonSteps;

		private InitializingScreen _initializingScreen;
		private SelectSdkScreen _selectSdkScreen;
		private SelectDatasourceScreen _selectDatasourceScreen;
		private LoginScreen _loginScreen;
		private LoadingScreen _loadingScreen;

		public LogonScreenView(ILogonScreenPresenter presenter)
		{
			_presenter = presenter;
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

			CurrentStep = LoginStep.Initializing;
			_presenter.InitializeLogin();
		}

		public void OkButtonClicked()
		{
			_presenter.OkbuttonClicked(CurrentStep);
		}

		public void CancelButtonClicked()
		{
			DialogResult = DialogResult.Cancel;
		}

		public void BackButtonClicked()
		{
			_presenter.BackButtonClicked(CurrentStep);
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
			ShowInTaskbar = (int) CurrentStep != 0;
			updatePanel((UserControl) _logonSteps[(int) CurrentStep]);
			Refresh();
		}

		private void updatePanel(Control userControl)
		{
			pnlContent.Controls.Clear();
			pnlContent.Controls.AddRange(userControl.Controls.OfType<Control>().ToArray());
		}

		public void Exit()
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
