using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Teleopti.Analytics.Portal.AnalyzerAPI;

namespace Teleopti.Analytics.Portal.Analyzer
{
    public partial class CloseReport : ReportPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string id = Request.QueryString["id"];
            if (!string.IsNullOrEmpty(id))
            {
                CloseReportInstance(id);
                Console.WriteLine("Closed report " + id.ToString());
            }
        }
    }
}