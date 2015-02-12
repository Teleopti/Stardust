using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleViewModelData
	{
		public DateOnly ShiftTradeDate { get; set; }

		public IList<Guid> TeamIdList { get; set; }

		public Paging Paging { get; set; }

		public TimeFilterInfo TimeFilter { get; set; }

		public string SearchNameText { get; set; }

		public string TimeSortOrder { get; set; }
	}

}