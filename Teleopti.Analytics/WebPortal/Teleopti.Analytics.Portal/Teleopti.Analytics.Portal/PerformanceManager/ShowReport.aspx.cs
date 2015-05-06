using System;
using System.Globalization;
using System.Web.UI.WebControls;
using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.PerformanceManager.Helper;
using Teleopti.Analytics.Portal.PerformanceManager.ViewModel;

namespace Teleopti.Analytics.Portal.PerformanceManager
{
    public partial class ShowReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AnalyzerReportView1.Visible = false;

				if (AuthorizationHelper.CurrentUserPmPermission(AuthorizationHelper.LoggedOnUser) == PermissionLevel.None)
            {
                return;
            }
            
            if (!IsPostBack)
            {
                bool isNewReport = false;
                string reportId = Request.QueryString["ReportId"];
                string reportName = Request.QueryString["ReportName"];

                if (!string.IsNullOrEmpty(Request.QueryString["new"]))
                {
                    if (Request.QueryString["new"] == "1")
                    {
                        isNewReport = true;
                    }
                }

                if (isNewReport)
                {
                    NewReport(reportName);
                }
                else
                {
                    OpenExistingReport(reportId);
                }

                SetReportTitle();
            }
        }

        private void NewReport(string reportName)
        {
            if (!string.IsNullOrEmpty(reportName))
            {
                
                var newReportDataSource = new AnalyzerReportViewModel(reportName);
                if (newReportDataSource.DoReportAlreadyExist)
                {
                    AnalyzerReportView1.Visible = false;
                    SetMessage(newReportDataSource.Message);
                }
                else
                {
                    AnalyzerReportView1.Visible = true;
                    AnalyzerReportView1.DataSource = newReportDataSource;
                    ((Site1)Master).CurrentReportInstanceId = AnalyzerReportView1.DataSource.ReportInstance.Id;
                    ReportInstanceHandler.Add(AnalyzerReportView1.DataSource.ReportInstance);
                }
            }
            else
            {
                SetMessage("Creation of new report failed. Report name is invalid or missing.");
            }
        }

        private void OpenExistingReport(string reportId)
        {
            if (!string.IsNullOrEmpty(reportId))
            {
                int id_number = int.Parse(reportId, CultureInfo.InvariantCulture);
                AnalyzerReportView1.Visible = true;
                AnalyzerReportView1.DataSource = new AnalyzerReportViewModel(id_number);
                ((Site1) Master).CurrentReportInstanceId = AnalyzerReportView1.DataSource.ReportInstance.Id;
                ReportInstanceHandler.Add(AnalyzerReportView1.DataSource.ReportInstance);
            }
            else
            {
                SetMessage("Open of report failed. Report number is invalid or missing.");
            }
        }

        private void SetReportTitle()
        {
            // Set report title
            var contentPlaceHolder = (ContentPlaceHolder)Master.FindControl("reportTitle");
            var labelReportHeader = new Label();
            labelReportHeader.Text = "";
            labelReportHeader.CssClass = "reportTitle";
            
            if (AnalyzerReportView1.DataSource != null && !string.IsNullOrEmpty(AnalyzerReportView1.DataSource.Url))
            {
                if (!string.IsNullOrEmpty(Request.QueryString["ReportName"]))
                {
                    labelReportHeader.Text = Request.QueryString["ReportName"];
                }
            }

            contentPlaceHolder.Controls.Add(labelReportHeader);
        }

        private void SetMessage(string message)
        {
            ((Site1)Master).Message = message;
        }
    }
}