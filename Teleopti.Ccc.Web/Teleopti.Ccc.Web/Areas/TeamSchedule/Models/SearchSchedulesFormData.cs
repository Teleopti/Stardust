using System;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Global.Models;
using Teleopti.Interfaces.Domain;

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