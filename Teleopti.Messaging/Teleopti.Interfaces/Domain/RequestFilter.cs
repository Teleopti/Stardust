using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public enum RequestsSortingOrder
	{
		AgentNameAsc,
		AgentNameDesc,
		UpdatedOnAsc,
		UpdatedOnDesc,
		CreatedOnAsc,
		CreatedOnDesc,
		SubjectAsc,
		SubjectDesc,
		PeriodStartAsc,
		PeriodStartDesc,
		PeriodEndAsc,
		PeriodEndDesc
	}

	public class RequestFilter
	{
		public DateTimePeriod Period;
		public IEnumerable<IPerson> Persons;
		public IEnumerable<RequestType> RequestTypes;
		public IList<RequestsSortingOrder> SortingOrders;
		public IPaging Paging;
		public bool ExcludeRequestsOnFilterPeriodEdge;
	}



}
