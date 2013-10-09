using System;
using System.Collections.Generic;
using System.Configuration;
using System.Deployment.Application;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
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
		private IList<ILogonStep> _logonSteps;
		
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
					new SelectSdkScreen(_model),
					new SelectDatasourceScreen(_model),
					new LoginScreen(_model),
                    new SelectBuScreen(_model)
				};
            var result = ShowDialog();
		    return result != DialogResult.Cancel;
		}

	    public void ShowStep(LoginStep theStep, bool showBackButton)
	    {
            _logonSteps[(int)theStep].SetData(_model);
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

		public bool InitializeAndCheckStateHolder(string skdProxyName)
		{
			if (_model.GetConfigFromWebService)
			{
				if (!LogonInitializeStateHolder.GetConfigFromWebService(skdProxyName))
					return showError();
			}
			else
			{
				var nhibConfPath = ApplicationDeployment.IsNetworkDeployed
					                   ? ApplicationDeployment.CurrentDeployment.DataDirectory
					                   : ConfigurationManager.AppSettings["nhibConfPath"];
				var useMessageBroker = string.IsNullOrEmpty(ConfigurationManager.AppSettings["MessageBroker"]);

				if (!LogonInitializeStateHolder.GetConfigFromFileSystem(nhibConfPath, useMessageBroker))
					return showError();
			}
            if (!string.IsNullOrEmpty(LogonInitializeStateHolder.WarningMessage))
            {
                // ReSharper disable LocalizableElement
               MessageBox.Show(this, LogonInitializeStateHolder.WarningMessage, "Configuration warning", MessageBoxButtons.OK);
                // ReSharper restore LocalizableElement
            }
			return true;
		}

		private bool showError()
		{
			// ReSharper disable LocalizableElement
			ShowInTaskbar = true;
			MessageBox.Show(this,
			                string.Format(CultureInfo.CurrentCulture,
			                              "The system configuration could not be loaded from the server. Review error message and log files to troubleshoot this error.\n\n{0}",
			                              LogonInitializeStateHolder.ErrorMessage),
			                "Configuration error", MessageBoxButtons.OK);
			return false;
			// ReSharper restore LocalizableElement
		}

		private void updatePanel(Control userControl)
		{
			pnlContent.Controls.Clear();
			pnlContent.Controls.Add(userControl);
		    ActiveControl = userControl;
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

        public void ShowErrorMessage(string message, string caption)
        {
	        MessageBoxAdv.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error,
	                           MessageBoxDefaultButton.Button1,
	                           (RightToLeft == RightToLeft.Yes
		                            ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
		                            : 0));
        }

	    public DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton)
	    {
	        return ViewBase.ShowYesNoMessage(this, text, caption, defaultButton);
        }

		public void ShowWarningMessage(string message, string caption)
		{
			MessageBoxAdv.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning,
			                   MessageBoxDefaultButton.Button1,
			                   (RightToLeft == RightToLeft.Yes
				                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
				                    : 0));
		}

		private void buttonLogOnCancelClick(object sender, EventArgs e)
        {
            Exit(DialogResult.Cancel);
        }

        private void buttonLogOnOkClick(object sender, EventArgs e)
        {
            Presenter.OkbuttonClicked();
        }

        private void btnBackClick(object sender, EventArgs e)
        {
            Presenter.BackButtonClicked();
        }

        private void release()
        {
            foreach (var logonStep in _logonSteps)
            {
                logonStep.Release();
            }
            _logonSteps = null;
        }
	}
}
