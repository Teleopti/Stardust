using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
		public IDictionary<RequestFilterField, string> RequestFilters;
		public IList<RequestsSortingOrder> SortingOrders;
		public Paging Paging;
		public bool ExcludeRequestsOnFilterPeriodEdge;
		public bool OnlyIncludeRequestsStartingWithinPeriod;
		public bool ExcludeInvalidShiftTradeRequest;
	}
}
