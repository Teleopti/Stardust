using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PerformanceManager
{
    public partial class PerformanceManagerNavigator : AbstractNavigator
    {
        public PerformanceManagerNavigator()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                SetTexts();
                CheckPermissions();
            }
        }

        private void CheckPermissions()
        {
            toolStripDropDownNewReport.Visible = false;
            toolStripDropDownViewReports.Visible = false;

            if (TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport))
            {
                // If you have right to create report then you also are authorized to view reports.
                toolStripDropDownNewReport.Visible = true;
                toolStripDropDownViewReports.Visible = true;
            }
            if (TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport))
            {
                toolStripDropDownViewReports.Visible = true;
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Common.MessageDialogs.ShowError(System.Windows.Forms.Control,System.String,System.String)")]
		private void toolStripNewReport_Click(object sender, EventArgs e)
        {
            string forceFormsLogin = "true";
            if (StateHolderReader.Instance.StateReader.SessionScopeData.AuthenticationTypeOption == AuthenticationTypeOption.Windows)
                forceFormsLogin = "false";

            string bUnitID = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id.ToString();

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = "iexplore.exe";
            
            string matrixUrl = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["MatrixWebSiteUrl"] + "?pm=1&forceformslogin={0}&buid={1}";
            proc.StartInfo.Arguments = string.Format(CultureInfo.CurrentCulture, matrixUrl, forceFormsLogin, bUnitID);
            //proc.Start();
			try
			{
				proc.Start();
			}
			catch (System.ComponentModel.Win32Exception ex)
			{
				MessageDialogs.ShowError(this, ex.Message, "Cannot load Internet Explorer");
			}
        }
    }
}
