using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider
{
	public interface IReportsNavigationProvider
	{
		IList<ReportNavigationItem> GetNavigationItems();
	}
}