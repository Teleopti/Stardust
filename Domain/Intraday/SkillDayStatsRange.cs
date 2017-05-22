using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillDayStatsRange
	{
		public Guid SkillId { get; set; }
		public DateOnly SkillDayDate { get; set; }
		public DateTimePeriod RangePeriod { get; set; }

	}
}