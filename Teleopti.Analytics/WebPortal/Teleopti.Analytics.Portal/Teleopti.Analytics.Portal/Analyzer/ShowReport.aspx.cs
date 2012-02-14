using System;
using Teleopti.Analytics.Portal.AnalyzerAPI;

namespace Teleopti.Analytics.Portal.Analyzer
{
    public partial class ShowReport : ReportPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Expires = -1;
            if (!IsPostBack)
            {
                Open();
                //FillReportList();
                //btnOpen.Enabled = ddlReports.Items.Count > 0;
            }
        }

        //private void FillReportList()
        //{
        //    Analyzer2005 az = GetProxy();
        //    try
        //    {
        //        ListReports(az, ddlReports);
        //    }
        //    catch (Exception e)
        //    {
        //        Message = e.Message;
        //    }
        //    finally
        //    {
        //        DisposeProxy(az);
        //    }
        //}

        private void Open()
        {
            Analyzer2005 az = GetProxy();
            try
            {
                //int reportId = int.Parse(ddlReports.SelectedValue);
                ReportInstance ri = az.OpenReport(CurrentContext, ReportId);
                CheckState(ri);
                CurrentInstanceId = ri.Id;
                CacheInstance(ri);
                ReportUrl = az.GetReportInstanceUrl(CurrentContext, ri, string.Format("{0}/{1}", MachineName, VirtualDir));
                
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                Message = e.Message;
            }
            finally
            {
                DisposeProxy(az);
            }
        }

        //protected void btnOpen_Click(object sender, EventArgs e)
        //{
        //    Open();
        //}
    }
}
