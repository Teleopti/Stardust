using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class RequestViewModel
	{
		public string Subject { get; set; }
		public string Message { get; set; }
		public string AgentName { get; set; }
		public Guid Id { get; set; }
		public DateTime? UpdatedTime { get; set; }
		public DateTime? CreatedTime { get; set; }
		public string TypeText { get; set; }
		public RequestStatus Status { get; set; }
		public string StatusText { get; set; }
	}
}