using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class RequestViewModel
	{
		public string Subject { get; set; }
		public string Message { get; set; }
		public string AgentName { get; set; }
		public Guid PersonId { get; set; }
		public string TimeZone { get; set; }
		public Guid Id { get; set; }
		public int Seniority { get; set; }
		public DateTime PeriodStartTime { get; set; }
		public DateTime PeriodEndTime { get; set; }
		public DateTime? UpdatedTime { get; set; }
		public DateTime? CreatedTime { get; set; }
		public RequestType Type { get; set; }
		public string TypeText { get; set; }
		public RequestStatus Status { get; set; }
		public string StatusText { get; set; }
		public Description Payload { get; set; }
		public string Team { get; set; }
		public bool IsNew { get; set; }
		public bool IsPending { get; set; }
		public bool IsApproved { get; set; }
		public bool IsDenied { get; set; }
		public bool IsWaitlisted { get; set; }
		public string DenyReason { get; set; }
	}

	public class RequestListViewModel<T> where T : RequestViewModel
	{
		public int TotalCount;
		public int Skip;
		public int Take;
		public IEnumerable<T> Requests;
		public bool IsSearchPersonCountExceeded;
		public int MaxSearchPersonCount;
	}
}



