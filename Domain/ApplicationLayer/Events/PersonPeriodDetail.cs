using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[Serializable]
	public struct PersonPeriodDetail
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid TeamId { get; set; }
		public IEnumerable<PersonSkillDetail> PersonSkillDetails { get; set; }
	}
}