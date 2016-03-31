using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class AnalyticsPersonPeriodSkillsChangedEvent : EventWithInfrastructureContext
	{
		public int AnalyticsPersonPeriodId { get; set; }
		public ICollection<int> AnalyticsActiveSkillsId { get; set; }
		public ICollection<int> AnalyticsInactiveSkillsId { get; set; }
		public int AnalyticsBusinessUnitId { get; set; }
	}
}
