using System;
using System.Diagnostics;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PerformanceManager
{
    public partial class PerformanceManagerNavigator : AbstractNavigator
    {
	    private readonly string _matrixWebSiteUrl;
		private readonly IApplicationInsights _applicationInsights;

	    public PerformanceManagerNavigator(string matrixWebSiteUrl, IApplicationInsights applicationInsights)
        {
				_matrixWebSiteUrl = matrixWebSiteUrl;
			_applicationInsights = applicationInsights;
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

            if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport))
            {
                // If you have right to create report then you also are authorized to view reports.
                toolStripDropDownNewReport.Visible = true;
                toolStripDropDownViewReports.Visible = true;
            }
            if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport))
            {
                toolStripDropDownViewReports.Visible = true;
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Common.MessageDialogs.ShowError(System.Windows.Forms.Control,System.String,System.String)")]
		private void toolStripNewReport_Click(object sender, EventArgs e)
		{
			_applicationInsights.TrackEvent("Opened report in Performance Manager Module.");
			var forceFormsLogin = "true";
            if (LogonPresenter.AuthenticationTypeOption == AuthenticationTypeOption.Windows)
                forceFormsLogin = "false";

            var bUnitID = ((ITeleoptiIdentity)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity).BusinessUnitId.ToString();

            var proc = new Process {EnableRaisingEvents = false, StartInfo = {FileName = "iexplore.exe"}};

			var matrixUrl = _matrixWebSiteUrl + "/PmContainer.aspx?pm=1&forceformslogin={0}&buid={1}";
			matrixUrl = string.Format(CultureInfo.CurrentCulture, matrixUrl, forceFormsLogin, bUnitID);
			proc.StartInfo.Arguments = string.Format(CultureInfo.CurrentCulture, matrixUrl, forceFormsLogin, bUnitID);
            //proc.Start();
			try
			{
				proc.Start();
				//Process.Start(matrixUrl);
			}
			catch (System.ComponentModel.Win32Exception ex)
			{
				MessageDialogs.ShowError(this, ex.Message, "Cannot load Internet Explorer");
			}
        }
    }
}
