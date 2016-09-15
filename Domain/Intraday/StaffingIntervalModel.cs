using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class StaffingIntervalModel
	{
		public DateTime StartTime { get; set; }

		public double Agents { get; set; }
		public Guid SkillId { get; set; }
	}
}