

using System;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer.DTOs
{
	public class IntradayForcastedStaffingIntervalDTO
	{
		public DateTime StartTime { get; set; }
		public double Agents { get; set; }
		public Guid SkillId { get; set; }
	}
}
