using System;

namespace Teleopti.Analytics.Portal.PerformanceManager.View
{
    public partial class AddReportView : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Button1_Click(object sender, EventArgs e)
        {            
            string url = String.Format("ShowReport.aspx?reportname={0}&new=1", TextBox1.Text);

            Response.Redirect(url);
        }

        public bool Enabled
        {
            get
            {
                return TextBox1.Enabled;
            }
            set
            {
                TextBox1.Enabled = value;
                Button1.Enabled = value;
            }
        }
    }
}