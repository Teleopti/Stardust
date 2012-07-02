namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	using Models.Domain;

	public interface IWebReportUserInfoProvider
	{
		WebReportUserInformation GetUserInformation();
	}
}