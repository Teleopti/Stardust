using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonSkillRemovedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public double Proficiency { get; set; }
		public bool SkillActive { get; set; }
		public Guid SkillId { get; set; }

		public IEnumerable<PersonSkillDetail> SkillsBefore { get; set; }
	}
}