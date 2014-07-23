using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleViewModelDataForAllTeams
	{
		public DateOnly ShiftTradeDate { get; set; }

		public IList<Guid> TeamIds { get; set; }

		public Paging Paging { get; set; }
	}
}