namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain
{
	public static class ReportIntervalTypeExtensions
	{
		public static bool IsTypeWeek(this ReportIntervalType val)
		{
			return val == ReportIntervalType.Week;
		}
	}

	public enum ReportIntervalType
	{
		NotDefined = 0,
		Day = 1,
		Week = 7
	}
}