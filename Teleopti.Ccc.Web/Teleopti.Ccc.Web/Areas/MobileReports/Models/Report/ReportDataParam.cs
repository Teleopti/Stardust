using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Report
{
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

	public class ReportDataParam
	{
		public DateOnlyPeriod Period { get; set; }

		public ReportIntervalType IntervalType { get; set; }

		public string SkillSet { get; set; }
	}
}