using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonPeriodDetail
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid SiteId { get; set; }
		public Guid TeamId { get; set; }
		public ICollection<PersonSkillDetail> PersonSkillDetails { get; set; }
	}
}