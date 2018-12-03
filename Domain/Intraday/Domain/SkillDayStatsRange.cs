using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class SkillDayStatsRange
	{
		public Guid SkillId { get; set; }
		public DateOnly SkillDayDate { get; set; }
		public DateTimePeriod RangePeriod { get; set; }

	}
}