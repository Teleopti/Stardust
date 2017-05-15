using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public interface IReportGenerator
	{
		GeneratedReport GenerateReport(Guid reportId, string connectionString, IList<SqlParameter> parameters,
			IList<string> parametersText, Guid userCode, Guid businessUnitCode, ReportGenerator.ReportFormat format, TimeZoneInfo userTimeZone);
	}
}