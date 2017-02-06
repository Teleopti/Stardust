using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class WorkShiftSelectorForMaxSeat : IWorkShiftSelector
	{
		private const double shiftNotToUse = double.MaxValue;
		private readonly IUsedSeats _usedSeats;
		private readonly IsAnySkillOpen _isAnySkillOpen;

		public WorkShiftSelectorForMaxSeat(IUsedSeats usedSeats, IsAnySkillOpen isAnySkillOpen)
		{
			_usedSeats = usedSeats;
			_isAnySkillOpen = isAnySkillOpen;
		}

		public virtual IShiftProjectionCache SelectShiftProjectionCache(IGroupPersonSkillAggregator groupPersonSkillAggregator, DateOnly datePointer, IList<IShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays,
			ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, TimeZoneInfo timeZoneInfo, bool forRoleModel, IPerson person)
		{
			var bestShiftValue = double.MaxValue;
			IShiftProjectionCache ret = null;

			var skillDays = allSkillDays.Where(x => x.CurrentDate == datePointer || x.CurrentDate == datePointer.AddDays(-1) || x.CurrentDate == datePointer.AddDays(1)).ToArray();
			var hasNonMaxSeatSkills = skillDays.Any(x => !(x.Skill is MaxSeatSkill));
			var maxSeatSkillDays = skillDays.Where(x => x.Skill is MaxSeatSkill).ToArray();

			foreach (var shift in shifts)
			{
				var thisShiftsPeak = thisShiftsValue(person, maxSeatSkillDays, shift, hasNonMaxSeatSkills, skillDays, bestShiftValue);

				if (thisShiftsPeak < bestShiftValue)
				{
					ret = shift;
					bestShiftValue = thisShiftsPeak;
				}
			}

			return ret;
		}

		private double thisShiftsValue(IPerson person, IEnumerable<ISkillDay> maxSeatSkillDays, IShiftProjectionCache shift, 
			bool hasNonMaxSeatSkills, IEnumerable<ISkillDay> skillDays, double bestShiftValue)
		{
			var thisShiftsPeak = 0d;

			foreach (var maxSeatSkillDay in maxSeatSkillDays)
			{
				foreach (var skillStaffPeriod in maxSeatSkillDay.SkillStaffPeriodCollection)
				{
					foreach (var layer in shift.MainShiftProjection.Where(x => x.Period.Intersect(skillStaffPeriod.Period)))
					{
						if (hasNonMaxSeatSkills && !_isAnySkillOpen.Check(skillDays, layer, person.PermissionInformation.DefaultTimeZone()))
						{
							return shiftNotToUse;
						}

						var addExtraDueToRequiresSeat = ((IActivity)layer.Payload).RequiresSeat ? 1 : 0;
						var occupiedSeatsThisInterval = _usedSeats.Fetch(skillStaffPeriod) + addExtraDueToRequiresSeat;
						thisShiftsPeak = Math.Max(thisShiftsPeak, occupiedSeatsThisInterval);
						if (thisShiftsPeak >= bestShiftValue)
						{
							return shiftNotToUse;
						}
					}
				}
			}
			return thisShiftsPeak;
		}
	}
}