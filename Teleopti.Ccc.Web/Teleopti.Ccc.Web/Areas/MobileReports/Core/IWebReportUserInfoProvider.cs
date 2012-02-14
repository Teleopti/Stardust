using Teleopti.Ccc.Web.Areas.MobileReports.Models;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public interface IWebReportUserInfoProvider
	{
		WebReportUserInformation GetUserInformation();
	}
}