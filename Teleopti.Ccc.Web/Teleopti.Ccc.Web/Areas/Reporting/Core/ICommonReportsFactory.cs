using System;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public interface ICommonReportsFactory
	{
		ICommonReports CreateAndLoad(string connectionString, Guid reportId);
	}
}