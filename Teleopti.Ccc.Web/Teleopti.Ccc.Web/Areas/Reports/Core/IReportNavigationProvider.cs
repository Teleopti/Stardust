using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.Reports.Models;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public interface IReportNavigationProvider
	{
		IList<ReportItemViewModel> GetNavigationItemViewModels();
		IList<CategorizedReportItemViewModel> GetCategorizedNavigationsItemViewModels();

		IList<ReportNavigationItem> GetNavigationItems();
	}
}