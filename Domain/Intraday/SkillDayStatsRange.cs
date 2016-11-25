using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillDayStatsRange
	{
		public Guid skillId { get; set; }
		public Guid skillDayId { get; set; }
		public DateTimePeriod RangePeriod { get; set; }

	}
}