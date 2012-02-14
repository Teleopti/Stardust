using System;
//using System.Collections;
//using System.Configuration;
//using System.Data;
//using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;

namespace Teleopti.Analytics.Portal
{
    public partial class Default : MatrixBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        { 
            if (ReportId == 0 | BusinessUnitCode == new Guid())
            {
                labelInformation.Text = "xxNo report or no business unit given!";
                Response.End();
            }
            else
            {
                Utils.CommonReports repUtil = new Utils.CommonReports(ConnectionString, ReportId);
                if (ReportId == 5 | ReportId == 6)
                {
                    Server.Transfer(repUtil.Url.ToString());
                }
                else
                {
                    Server.Transfer(repUtil.Url.ToString() + "&buid=" + BusinessUnitCode.ToString());
                }
                
            }
        }
    }
}
