using System;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer.DTOs
{
	public class IntradayScheduleStaffingIntervalDTO
	{
		public Guid SkillId { get; set; }
		public DateTime StartDateTimeUtc { get; set; }
		public DateTime EndDateTimeUtc { get; set; }
		public double StaffingLevel { get; set; }
	}
}
