using System;
using System.Web;
using System.Web.Configuration;
using Teleopti.Analytics.ReportTexts;

namespace Teleopti.Analytics.Portal
{
    public partial class ErrorMessage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int errorId = -1;
            if (!string.IsNullOrEmpty(Request.QueryString["id"]))
            {
                try
                {
                    errorId = int.Parse(Request.QueryString["id"]);
                }
                catch (FormatException){}
                catch (OverflowException){}
            }

            ShowError(errorId);
            _labelPossibleSolutionHeader.Text = Resources.PossibleSolution;
            _labelTechnicalDetailHeader.Text = Resources.TechnicalInformation;
        }

        private void ShowError(int errorId)
        {
            string errorMessage;
            string possibleSolution = "";

            switch (errorId)
            {
                case 1:
                    errorMessage = Resources.AuthenticationFailedForUser + ".";
                    possibleSolution = Resources.PossibleSolution1;  //"xxMake sure the permissions are synchronized properly with the ETL service.";
                    break;
                case 2:
                    //errorMessage = //"xxAuthentication failed for user. When using Windows authentication at the web application it is not valid to use Application authentication at the Client side.";
                    errorMessage = string.Concat(Resources.AuthenticationFailedForUser, ". ",
                                                 Resources.AuthenticationMixedFailure);
                    possibleSolution = Resources.PossibleSolution2; //"xxEither close the client and log back on using Windows authentication or a system administrator can change web authentication to Forms.";
                    break;
                default:
                    errorMessage = Resources.UnknownError;
                    break;
            }

            _labelErrorMessage.Text = errorMessage;

            ShowTechnicalDetails(possibleSolution);
        }

        private void ShowTechnicalDetails(string possibleSolution)
        {
            _labelUser.Text = string.Concat("Current user: ", Request.ServerVariables["LOGON_USER"]);

            var sec = (AuthenticationSection)HttpContext.Current.GetSection("system.web/authentication");
            AuthenticationMode webAuthenticationMode = sec.Mode;

            _labelWebAuthMode.Text = string.Format("Web authentication type: {0}", webAuthenticationMode);

            if (!string.IsNullOrEmpty(Request.QueryString["win"]))
            {
                string clientAuthenticationMode = "Application";
                if (Request.QueryString["win"] == "True")
                {
                    clientAuthenticationMode = "Windows";
                }
                _labelClientAuthMode.Text = string.Format("Client authentication type: {0}", clientAuthenticationMode);
            }
            else
            {
                _labelClientAuthMode.Text = string.Format("Client authentication type: {0}", "Unknown");
            }

            _labelPossibleSolution.Text = possibleSolution;
        }
    }
}
