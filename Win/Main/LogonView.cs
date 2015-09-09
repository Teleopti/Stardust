using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Autofac;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
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
			((UserControl) currentStep).Focus();
		}

		public void ClearForm(string labelText)
		{
			pnlContent.Controls.Clear();
			pnlContent.Visible = false;
			
			Refresh();
		}

		public void InitStateHolderWithoutDataSource(IMessageBrokerComposite messageBroker, SharedSettings settings)
		{
			LogonInitializeStateHolder.InitWithoutDataSource(messageBroker, settings);
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

		public void ButtonLogOnOkClick(object sender, EventArgs e)
		{
			_logonSteps[(int) Presenter.CurrentStep].GetData();
			Presenter.OkbuttonClicked();
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

		private void LogonView_Load(object sender, EventArgs e)
		{
			Show();
			Refresh();
		}
	}
}
