using System;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal
{
    public partial class Default : MatrixBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (ReportId == new Guid() | BusinessUnitCode == new Guid())
            {
                labelInformation.Text = "xxNo report or no business unit given!";
                Response.End();
            }
            else
            {
                var repUtil = new CommonReports(ConnectionString, ReportId);
                //if (ReportId == 5 | ReportId == 6)
                //{
                //    Server.Transfer(repUtil.Url.ToString());
                //}
                //else
                {
                    Server.Transfer(repUtil.Url + "&buid=" + BusinessUnitCode);
                }
                
            }
        }
    }
}
