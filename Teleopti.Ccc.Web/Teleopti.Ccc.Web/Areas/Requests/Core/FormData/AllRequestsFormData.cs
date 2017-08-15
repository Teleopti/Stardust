using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class AllRequestsFormData
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IList<RequestsSortingOrder> SortingOrders { get; set; }
		public IDictionary<PersonFinderField, string> AgentSearchTerm { get; set; }
		public IDictionary<RequestFilterField, string> Filters { get; set; }
		public Paging Paging { get; set; }
		public Guid[] SelectedGroupIds { get; set; }
	}
}