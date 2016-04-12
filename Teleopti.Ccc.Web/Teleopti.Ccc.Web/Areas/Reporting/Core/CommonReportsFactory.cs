using System;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class CommonReportsFactory: ICommonReportsFactory
	{
		public ICommonReports CreateAndLoad(string connectionString, Guid reportId)
		{
			var commonReports = new CommonReports(connectionString, reportId);
			commonReports.LoadReportInfo();
			return commonReports;
		}
	}
}