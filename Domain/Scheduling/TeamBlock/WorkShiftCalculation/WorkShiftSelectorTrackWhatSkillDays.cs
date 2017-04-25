using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class WorkShiftSelectorTrackWhatSkillDays : WorkShiftSelectorForMaxSeat
	{

		public WorkShiftSelectorTrackWhatSkillDays(IUsedSeats usedSeats, IsAnySkillOpen isAnySkillOpen)
			:base(usedSeats, isAnySkillOpen)
		{
		}

		public IEnumerable<ISkillDay> SkillDaysAsParameter { get; private set; }

		public override ShiftProjectionCache SelectShiftProjectionCache(IGroupPersonSkillAggregator groupPersonSkillAggregator,
			DateOnly datePointer, IList<ShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays, ITeamBlockInfo teamBlockInfo,
			SchedulingOptions schedulingOptions, TimeZoneInfo timeZoneInfo, bool forRoleModel, IPerson person)
		{
			SkillDaysAsParameter = allSkillDays;
			return base.SelectShiftProjectionCache(groupPersonSkillAggregator, datePointer, shifts, allSkillDays, teamBlockInfo, schedulingOptions, timeZoneInfo, forRoleModel, person);
		}
	}
}