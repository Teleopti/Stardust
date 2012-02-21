using Teleopti.Ccc.Web.Areas.MobileReports.Models;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

	public interface IWebReportUserInfoProvider
	{
		WebReportUserInformation GetUserInformation();
	}
}