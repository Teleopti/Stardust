using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class WorkShiftSelectorForMaxSeat : IWorkShiftSelector
	{
		private readonly IUsedSeats _usedSeats;
		private readonly IsAnySkillOpen _isAnySkillOpen;

		public WorkShiftSelectorForMaxSeat(IUsedSeats usedSeats, IsAnySkillOpen isAnySkillOpen)
		{
			_usedSeats = usedSeats;
			_isAnySkillOpen = isAnySkillOpen;
		}

		public IShiftProjectionCache SelectShiftProjectionCache(DateOnly datePointer, IList<IShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays,
			ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, TimeZoneInfo timeZoneInfo, bool forRoleModel)
		{
			var bestShiftValue = double.MaxValue;
			IShiftProjectionCache ret =null;

			var skillDays = allSkillDays.FilterOnDate(datePointer);
			var hasNonMaxSeatSkills = skillDays.Any(x => !(x.Skill is MaxSeatSkill));

			foreach (var shift in shifts)
			{
				var thisShiftsPeak = 0d;

				foreach (var layer in shift.MainShiftProjection)
				{
					var activity = (IActivity) layer.Payload;
					var thisShiftRequiresOneSeatExtra = activity.RequiresSeat;

					if (hasNonMaxSeatSkills && !_isAnySkillOpen.Check(skillDays, layer))
					{
						thisShiftsPeak = double.MaxValue;
						break;
					}

					foreach (var maxSeatSkillDay in skillDays.Where(x => x.Skill is MaxSeatSkill))
					{
						foreach (var interval in layer.Period.Intervals(TimeSpan.FromMinutes(maxSeatSkillDay.Skill.DefaultResolution)))
						{
							var skillStaffPeriod = maxSeatSkillDay.SkillStaffPeriodCollection.Single(x => x.Period == interval);
							var occupiedSeatsThisInterval = _usedSeats.Fetch(skillStaffPeriod);
							if (thisShiftRequiresOneSeatExtra)
							{
								occupiedSeatsThisInterval++;
							}
							thisShiftsPeak = Math.Max(thisShiftsPeak, occupiedSeatsThisInterval);
						}
					}
				}
				if (thisShiftsPeak < bestShiftValue)
				{
					ret = shift;
					bestShiftValue = thisShiftsPeak;
				}
			}

			return ret;
		}
	}
}