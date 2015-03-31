using System;
using System.Collections.Generic;
using System.Configuration;
using System.Deployment.Application;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Main.LogonScreens;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LogonView : MetroForm, ILogonView
	{
		private readonly LogonModel _model;
		public ILogonPresenter Presenter { get; set; }
		private IList<ILogonStep> _logonSteps;

		public LogonView(LogonModel model)
		{
			_model = model;
			InitializeComponent();
			labelVersion.Text = string.Concat("Version ", Application.ProductVersion);
		}

		public bool StartLogon(IMessageBrokerComposite messageBroker)
		{
			_logonSteps = new List<ILogonStep>
				{
					new SelectSdkScreen(this, _model),
					new SelectDatasourceScreen(this, _model),
					new LoginScreen(this, _model),
					new SelectBuScreen(this, _model)
				};
			DialogResult result = ShowDialog();
			return result != DialogResult.Cancel;
		}

		public void ShowStep(bool showBackButton)
		{
			var currentStep = _logonSteps[(int) Presenter.CurrentStep];
			currentStep.SetData();
			updatePanel((UserControl) currentStep);
			currentStep.SetBackButtonVisible(showBackButton);
			
			Refresh();

		}

		public void ClearForm(string labelText)
		{
			pnlContent.Controls.Clear();
			pnlContent.Visible = false;
			
			Refresh();
		}

		public bool InitStateHolderWithoutDataSource(IMessageBrokerComposite messageBroker, SharedSettings settings)
		{
			if (_model.GetConfigFromWebService)
			{
				if (!LogonInitializeStateHolder.InitWithoutDataSource(_model, messageBroker, settings))
					return showError();
			}
			else
			{
				//used by sikuli
				var useMessageBroker = string.IsNullOrEmpty(ConfigurationManager.AppSettings["MessageBroker"]);

				if (!LogonInitializeStateHolder.GetConfigFromFileSystem(Environment.CurrentDirectory, useMessageBroker, messageBroker))
					return showError();
			}
			return true;
		}

		private bool showError()
		{
			ShowInTaskbar = true;
			MessageBox.Show(this,
			                string.Format(CultureInfo.CurrentCulture,
			                              "The system configuration could not be loaded from the server. Review error message and log files to troubleshoot this error.\n\n{0}",
			                              LogonInitializeStateHolder.ErrorMessage),
			                "Configuration error", MessageBoxButtons.OK);
			return false;
		}

		private void updatePanel(Control userControl)
		{
			pnlContent.Controls.Clear();
			pnlContent.Controls.Add(userControl);
			userControl.Dock = DockStyle.Fill;
			ActiveControl = userControl;
			pnlContent.Visible = true;
		}

		public void Exit(DialogResult result)
		{
			DialogResult = result;
			release();
			Close();
			Dispose();
		}

		public void Warning(string warning)
		{
			Warning(warning, Resources.LogOn);
		}

		public void Warning(string warning, string caption)
		{
			ShowInTaskbar = true;
			MessageDialogs.ShowWarning(this, warning, caption);
			ShowInTaskbar = false;

			DialogResult = DialogResult.None;
		}

		public void Error(string error)
		{
			showApplyProductActivationKeyDialogAndExit(error);
		}

		private void showApplyProductActivationKeyDialogAndExit(string explanation)
		{
			var applyProductActivationKey = new ApplyProductActivationKey(explanation, _model.SelectedDataSourceContainer.DataSource.Application);
			applyProductActivationKey.ShowDialog(this);
			Application.Exit();
		}

		private void logonViewShown(object sender, EventArgs e)
		{
			//We must call back so we not just hang
			Presenter.Initialize();
		}

		public void ShowErrorMessage(string message, string caption)
		{
			MessageDialogs.ShowWarning(this, message, caption);
		}

		public DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton)
		{
			return ViewBase.ShowYesNoMessage(this, text, caption, defaultButton);
		}

		public void ShowWarningMessage(string message, string caption)
		{
			MessageDialogs.ShowWarning(this, message, caption);
		}

		public void ButtonLogOnOkClick(object sender, EventArgs e)
		{
			_logonSteps[(int) Presenter.CurrentStep].GetData();
			Presenter.OkbuttonClicked();
		}

		public void ButtonLogOnCancelClick(object sender, EventArgs e)
		{
			Exit(DialogResult.Cancel);
		}

		public void BtnBackClick(object sender, EventArgs e)
		{
			Presenter.BackButtonClicked();
		}

		private void release()
		{
			foreach (var logonStep in _logonSteps)
				logonStep.Release();
			_logonSteps = null;
		}

		public void HandleKeyPress(Message msg, Keys keyData, bool shouldGoFoward)
		{
			const int wmKeydown = 0x100;
			const int wmSyskeydown = 0x104;

			if ((msg.Msg != wmKeydown) && (msg.Msg != wmSyskeydown)) return;

			switch (keyData)
			{
				case Keys.Enter:
					if (shouldGoFoward)
						ButtonLogOnOkClick(this, EventArgs.Empty);

					break;
			}
		}
	}
}
