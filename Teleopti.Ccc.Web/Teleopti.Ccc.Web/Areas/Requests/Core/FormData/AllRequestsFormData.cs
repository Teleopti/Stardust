using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class AllRequestsFormData: SearchGroupIdsData
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IList<RequestsSortingOrder> SortingOrders { get; set; }
		public IDictionary<PersonFinderField, string> AgentSearchTerm { get; set; }
		public IDictionary<RequestFilterField, string> Filters { get; set; }
		public Paging Paging { get; set; }
		
	}

	public class ShiftTradeScheduleForm
	{
		public Guid PersonFromId { get; set; }
		public Guid PersonToId { get; set; }
		public DateTime RequestDate { get; set; }
	}
}