using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public struct SkillStaffingIntervalLightModel : IEquatable<SkillStaffingIntervalLightModel>
	{
		public Guid Id { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double StaffingLevel { get; set; }

		public bool Equals(SkillStaffingIntervalLightModel other)
		{
			return Id == other.Id
				   && StartDateTime == other.StartDateTime
				   && EndDateTime == other.EndDateTime
				   && Math.Abs(StaffingLevel - other.StaffingLevel) < 0.001;
		}
	}
}