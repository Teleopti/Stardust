using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public interface IDefinedReportProvider
	{
		IEnumerable<DefinedReportInformation> GetDefinedReports();
		IDefinedReport Get(string reportId);
	}
}