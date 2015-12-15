using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class AllRequestsFormData
	{
		public DateOnly StartDate;
		public DateOnly EndDate;
		public IList<RequestsSortingOrder> SortingOrders = new List<RequestsSortingOrder>();
		public string AgentSearchTerm;
		public Paging Paging = new Paging();
	}
}