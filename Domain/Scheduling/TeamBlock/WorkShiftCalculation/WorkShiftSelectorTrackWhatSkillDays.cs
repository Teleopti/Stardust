using System;
using System.Collections.Generic;
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

		public override IShiftProjectionCache SelectShiftProjectionCache(IGroupPersonSkillAggregator groupPersonSkillAggregator,
			DateOnly datePointer, IList<IShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays, ITeamBlockInfo teamBlockInfo,
			ISchedulingOptions schedulingOptions, TimeZoneInfo timeZoneInfo, bool forRoleModel, IPerson person)
		{
			SkillDaysAsParameter = allSkillDays;
			return base.SelectShiftProjectionCache(groupPersonSkillAggregator, datePointer, shifts, allSkillDays, teamBlockInfo, schedulingOptions, timeZoneInfo, forRoleModel, person);
		}
	}
}