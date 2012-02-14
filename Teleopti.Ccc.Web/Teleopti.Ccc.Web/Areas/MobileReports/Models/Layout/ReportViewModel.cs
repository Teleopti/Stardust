using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout
{
	public class ReportViewModel
	{
		public DateBoxGlobalizationViewModel DateBoxGlobalization { get; set; }
		public IEnumerable<ReportSelectionViewModel> Reports { get; set; }
		public IEnumerable<SkillSelectionViewModel> Skills { get; set; }
	}
}