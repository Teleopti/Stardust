using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Report
{
	using System;

	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

	public class ReportDataParam
	{
		public Guid ReportId;

		public DateOnlyPeriod Period { get; set; }

		public ReportIntervalType IntervalType { get; set; }

		public string SkillSet { get; set; }
	}
}