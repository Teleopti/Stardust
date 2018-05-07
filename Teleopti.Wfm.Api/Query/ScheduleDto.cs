using System;

namespace Teleopti.Wfm.Api.Query
{
	public class ScheduleDto
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public ShiftLayerDto[] Shift { get; set; }
	}
}