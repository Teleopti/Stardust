using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Reports.Models;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public interface IReportNavigationProvider
	{
		IList<ReportItem> GetNavigationItems();
		IList<CategorizedReportItem> GetCategorizedNavigationsItems();
	}
}