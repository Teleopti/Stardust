using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Main.LogonScreens;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LogonView : Form, ILogonView
	{
	    private readonly LogonModel _model;
	    public ILogonPresenter Presenter{get; set; }
		private  IList<ILogonStep> _logonSteps;
		
		public LogonView(LogonModel model)
		{
		    _model = model;
		    InitializeComponent();
            buttonLogOnCancel.Text = Resources.Cancel;
            buttonLogOnOK.Text = Resources.Ok;
		    btnBack.Text = Resources.Back;
		}

		public bool StartLogon()
		{
          _logonSteps = new List<ILogonStep>
				{
					new SelectSdkScreen(this, _model),
					new SelectDatasourceScreen(this, _model),
					new LoginScreen(this, _model),
                    new SelectBuScreen(this, _model)
				};
            var result = ShowDialog();
		    return result != DialogResult.Cancel;
		}

	    public void ShowStep(LoginStep theStep, LogonModel model, bool showBackButton)
	    {
            _logonSteps[(int)theStep].SetData(model);
            updatePanel((UserControl)_logonSteps[(int)theStep]);
            labelStatusText.Visible = false;
	        buttonLogOnCancel.Visible = true;
	        buttonLogOnOK.Visible = true;
	        btnBack.Visible = showBackButton;
            Refresh();

	    }

	    public void ClearForm(string labelText)
	    {
            pnlContent.Controls.Clear();
            labelStatusText.Text = labelText;
            labelStatusText.Visible = labelText != "";
            buttonLogOnCancel.Visible = false;
            buttonLogOnOK.Visible = false;
            btnBack.Visible = false;
            Refresh();
	    }

        //Dessa till resurs eller vet vi ändå inte språket, kan man använda datorns???
	    public bool InitializeAndCheckStateHolder(string skdProxyName)
        {
            if (!LogonInitializeStateHolder.GetConfigFromWebService(skdProxyName))
            {
                ShowInTaskbar = true;
                MessageBox.Show(this,
                                string.Format(CultureInfo.CurrentCulture,
                                              "The system configuration could not be loaded from the server. Review error message and log files to troubleshoot this error.\n\n{0}",
                                              LogonInitializeStateHolder.ErrorMessage),
                                "Configuration error", MessageBoxButtons.OK);
                return false;
            }
            if (!string.IsNullOrEmpty(LogonInitializeStateHolder.WarningMessage))
            {
                ShowInTaskbar = true;
                MessageBox.Show(this, LogonInitializeStateHolder.WarningMessage, "Configuration warning", MessageBoxButtons.OK);
                ShowInTaskbar = false;
            }
            return true;
        }

		private void updatePanel(Control userControl)
		{
			pnlContent.Controls.Clear();
			pnlContent.Controls.Add(userControl);
		    ActiveControl = userControl;
		}
		
		public void Exit(DialogResult result)
		{
            //vi måste nog disposa ngnstans
		    DialogResult = result;
			Close();
		}

        public void Warning(string warning)
        {
            ShowInTaskbar = true;
            MessageDialogs.ShowWarning(this, warning, Resources.LogOn);
            ShowInTaskbar = false;

            DialogResult = DialogResult.None;
        }

        public void Error(string error)
        {
            showApplyLicenseDialogAndExit(error);
        }

        private void showApplyLicenseDialogAndExit(string explanation)
        {
            var applyLicense = new ApplyLicense(explanation, _model.SelectedDataSourceContainer.DataSource.Application);
            applyLicense.ShowDialog(this);
            Application.Exit();
        }

        private void logonViewShown(object sender, EventArgs e)
        {
            //We must call back so we not just hang
            Presenter.Initialize();
        }

        public void ShowErrorMessage(string message)
        {
            MessageBoxAdv.Show(message, Resources.ErrorMessage, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));
        }

        private void buttonLogOnCancelClick(object sender, EventArgs e)
        {
            Exit(DialogResult.Cancel);
        }

        private void buttonLogOnOkClick(object sender, EventArgs e)
        {
            Presenter.OkbuttonClicked(_logonSteps[(int)Presenter.CurrentStep].GetData());
        }

        private void btnBackClick(object sender, EventArgs e)
        {
            Presenter.BackButtonClicked();
        }

	}
}
