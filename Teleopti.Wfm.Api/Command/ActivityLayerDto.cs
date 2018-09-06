using System;

namespace Teleopti.Wfm.Api.Command
{
	public class ActivityLayerDto
	{
		public DateTime UtcStartDateTime { get; set; }
		public DateTime UtcEndDateTime { get; set; }
		public Guid ActivityId { get; set; }
	}
}