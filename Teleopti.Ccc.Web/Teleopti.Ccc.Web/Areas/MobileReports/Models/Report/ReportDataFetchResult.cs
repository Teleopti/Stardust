using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Report
{
	public class ReportDataFetchResult
	{
		public IEnumerable<string> Errors { get; set; }

		public ReportGenerationResult GenerationRequest { get; set; }

		public bool IsValid()
		{
			return !(Errors != null && Errors.Any());
		}
	}
}