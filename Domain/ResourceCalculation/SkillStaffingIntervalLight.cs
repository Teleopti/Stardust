using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public struct SkillStaffingIntervalLight : IEquatable<SkillStaffingInterval>
	{
		public Guid SkillId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double StaffingLevel { get; set; }

		public bool Equals(SkillStaffingInterval other)
		{
			return SkillId == other.SkillId
				   && StartDateTime == other.StartDateTime
				   && EndDateTime == other.EndDateTime
				   && Math.Abs(StaffingLevel - other.StaffingLevel) < 0.001;
		}
	}
}