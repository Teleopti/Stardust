using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[Serializable]
	public struct PersonSkillDetail
	{
		public Guid SkillId { get; set; }
		public double Proficiency { get; set; }
		public bool Active { get; set; }
	}
}