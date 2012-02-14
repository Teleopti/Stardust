using System;

namespace Teleopti.Analytics.Portal.PerformanceManager
{
    public partial class CloseReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string id = Request.QueryString["id"];
            if (!string.IsNullOrEmpty(id))
            {
                // Close report and remove its instance from cache
                ReportInstanceHandler.Remove(id);
            }
        }
    }
}
