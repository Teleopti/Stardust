using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class AllRequestsFormData
	{
		public DateOnly StartDate;
		public DateOnly EndDate;
		public IList<RequestsSortingOrder> SortingOrders = new List<RequestsSortingOrder>();
		public string AgentSearchTerm;
	}

	public enum RequestsSortingOrder
	{
		AgentNameAsc,
		AgentNameDesc,
		UpdatedOnAsc,
		UpdatedOnDesc,
		CreatedOnAsc,
		CreatedOnDesc
	}
}