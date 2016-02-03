using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonPeriodDetail
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public ICollection<PersonSkillDetail> PersonSkillDetails { get; set; }
	}
}