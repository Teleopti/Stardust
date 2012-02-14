using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Report
{
	public class ReportDataParam
	{
		public DateOnlyPeriod Period { get; set; }

		public int IntervalType { get; set; }

		public string SkillSet { get; set; }
	}
}