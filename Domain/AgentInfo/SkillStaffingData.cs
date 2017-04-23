using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class SkillStaffingData
	{
		public DateOnly Date { get; set; }
		public ISkill Skill { get; set; }
		public DateTime Time { get; set; }
		public double? ForecastedStaffing { get; set; }
		public double? ScheduledStaffing { get; set; }
		public int Resolution { get; set; }
	}
}