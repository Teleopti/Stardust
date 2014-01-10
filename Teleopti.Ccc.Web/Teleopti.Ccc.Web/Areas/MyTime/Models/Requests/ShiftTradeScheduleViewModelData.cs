using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleViewModelData
	{
		public DateOnly ShiftTradeDate { get; set; }

		public bool LoadOnlyMyTeam { get; set; }

		public Paging Paging { get; set; }
	}
}