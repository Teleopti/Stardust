using System;
using System.Web;

namespace Teleopti.Analytics.Portal.PerformanceManager.View
{
    public partial class NewView : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            buttonCreate.Click += new EventHandler(buttonCreate_Click);
        }

        void buttonCreate_Click(object sender, EventArgs e)
        {
            string reportName = HttpUtility.UrlEncode(inputTextName.Value);
            string url = String.Format("ShowReport.aspx?reportname={0}&new=1", reportName);

            Response.Redirect(url);
        }
    }
}