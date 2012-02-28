using System.ComponentModel.DataAnnotations;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Report
{
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

	/// <summary>
	/// 	Passed from ReportSettings -> View
	/// </summary>
	public class ReportRequestModel
	{
		/// <summary>
		/// 	Predefined as Passed from DefinedReportProvider
		/// </summary>
		[Required]
		public string ReportId { get; set; }

		/// <summary>
		/// 	Date, will be adjusted to match interval if needed
		/// </summary>
		[Required]
		public DateOnly ReportDate { get; set; }

		/// <summary>
		/// 	Skill Id's csv
		/// </summary>
		[Required]
		public string SkillSet { get; set; }

		/// <summary>
		/// 	Holds interval (days) 1 or 7
		/// </summary>
		[Required]
		public int ReportIntervalType { get; set; }
	}
}