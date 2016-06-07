using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public interface IReportNavigationProvider
	{
		IList<ReportNavigationItem> GetNavigationItems();
	}
}