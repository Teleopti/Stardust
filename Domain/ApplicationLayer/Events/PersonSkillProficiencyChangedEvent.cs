using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonSkillProficiencyChangedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid SkillId { get; set; }
		public double ProficiencyAfter { get; set; }

		public IEnumerable<PersonSkillDetail> SkillsBefore { get; set; }
	}
}