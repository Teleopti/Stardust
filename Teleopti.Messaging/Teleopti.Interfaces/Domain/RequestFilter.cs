using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{

	public enum RequestsSortingOrder
	{
		AgentNameAsc,
		AgentNameDesc,
		CreatedOnAsc,
		CreatedOnDesc,
		DenyReasonAsc,
		DenyReasonDesc,
		MessageAsc,
		MessageDesc,
		PeriodStartAsc,
		PeriodStartDesc,
		PeriodEndAsc,
		PeriodEndDesc,
		SeniorityAsc,
		SeniorityDesc,
		SubjectAsc,
		SubjectDesc,
		TeamAsc,
		TeamDesc,
		UpdatedOnAsc,
		UpdatedOnDesc		
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
