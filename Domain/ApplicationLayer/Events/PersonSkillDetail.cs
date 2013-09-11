using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonSkillDetail
	{
		public Guid SkillId { get; set; }
		public double Proficiency { get; set; }
		public bool Active { get; set; }
	}
}