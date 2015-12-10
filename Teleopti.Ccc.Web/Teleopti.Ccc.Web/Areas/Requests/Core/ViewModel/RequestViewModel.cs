using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class RequestViewModel

	{
		public string Subject { get; set; }
		public string Message { get; set; }
		public string AgentName { get; set; }
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
	}
}