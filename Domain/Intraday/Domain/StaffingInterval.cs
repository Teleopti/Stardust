using System;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class StaffingInterval
	{
		public DateTime StartTime { get; set; }
		public double Agents { get; set; }
		public Guid SkillId { get; set; }
	}
}