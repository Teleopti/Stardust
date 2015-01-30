using System;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal
{
    public partial class Default : MatrixBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (ReportId == Guid.Empty | BusinessUnitCode == Guid.Empty)
            {
                labelInformation.Text = "xxNo report or no business unit given!";
                Response.End();
            }
            else
            {
                var repUtil = new CommonReports(ConnectionString, ReportId);
                Server.Transfer(repUtil.Url + "&buid=" + BusinessUnitCode);
            }
        }
    }
}
