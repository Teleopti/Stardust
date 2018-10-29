using System.Collections.Generic;

namespace Teleopti.Wfm.Api.Query.Response
{
	public class ScheduleChangesListenerSubscriptionDto
	{
		public ICollection<ScheduleChangesListenerDto> Listeners { get; } = new List<ScheduleChangesListenerDto>();
		public string Modulus { get; set; }
		public string Exponent { get; set; }
	}
}