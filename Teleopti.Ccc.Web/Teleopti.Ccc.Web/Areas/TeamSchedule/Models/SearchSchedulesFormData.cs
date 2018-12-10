using Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class SearchSchedulesFormData : SearchGroupIdsData
	{
		public string Keyword { get; set; }
		public DateOnly Date { get; set; }
		public int PageSize { get; set; }
		public int CurrentPageIndex { get; set; }
		
	}

}