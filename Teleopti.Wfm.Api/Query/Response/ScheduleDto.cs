using System;

namespace Teleopti.Wfm.Api.Query.Response
{
	public class ScheduleDto
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public ShiftLayerDto[] Shift { get; set; }
		public string TimeZoneId { get; set; }
	}
}