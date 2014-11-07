using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class ReportItem
	{
		public string Url { get; set; }
		public string Name { get; set; }
	}

	public interface IReportItemsProvider
	{
		List<ReportItem> GetReportItems();
	}
}