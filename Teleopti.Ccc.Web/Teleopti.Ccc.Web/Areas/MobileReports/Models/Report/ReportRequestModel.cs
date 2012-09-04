using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Report
{
	/// <summary>
	/// 	Passed from ReportSettings -> View
	/// </summary>
	public class ReportRequestModel
	{
		/// <summary>
		/// 	Predefined as Passed from DefinedReportProvider
		/// </summary>
		[Required(ErrorMessageResourceName = "PleaseChooseAReportType", ErrorMessageResourceType = typeof(Resources))]
		public string ReportId { get; set; }

		/// <summary>
		/// 	Date, will be adjusted to match interval if needed
		/// </summary>
		[Required]
		public DateOnly ReportDate { get; set; }

		/// <summary>
		/// 	Skill Id's csv
		/// </summary>
		[Required(ErrorMessageResourceName = "PleaseChooseSkillSet", ErrorMessageResourceType = typeof(Resources))]
		public string SkillSet { get; set; }

		/// <summary>
		/// 	Holds interval (days) 1 or 7
		/// </summary>
		[Required]
		public int ReportIntervalType { get; set; }
	}
}