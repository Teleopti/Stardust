using System;

namespace Teleopti.Wfm.Api.Query
{
	public class ShiftLayerDto
	{
		public Guid PayloadId { get; set; }
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}