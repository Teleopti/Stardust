using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class StaffingIntervalChange : IEquatable<StaffingIntervalChange>
	{
		public Guid SkillId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double StaffingLevel { get; set; }


		public bool Equals(StaffingIntervalChange other)
		{
			if (other == null) return false;
			return SkillId == other.SkillId
				   && StartDateTime == other.StartDateTime
				   && EndDateTime == other.EndDateTime
				   && Math.Abs(StaffingLevel - other.StaffingLevel) < 0.001;
		}
	}
}