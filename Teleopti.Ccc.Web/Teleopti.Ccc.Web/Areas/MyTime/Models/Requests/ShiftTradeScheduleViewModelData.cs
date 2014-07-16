using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleViewModelData
	{
		public DateOnly ShiftTradeDate { get; set; }

		public Guid TeamId { get; set; }

		public Paging Paging { get; set; }

		public int PageIndex { get; set; }
	}
}