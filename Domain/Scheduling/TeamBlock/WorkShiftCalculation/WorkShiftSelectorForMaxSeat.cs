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

		public WorkShiftSelectorForMaxSeat(IUsedSeats usedSeats)
		{
			_usedSeats = usedSeats;
		}

		public IShiftProjectionCache SelectShiftProjectionCache(DateOnly datePointer, IList<IShiftProjectionCache> shifts, IEnumerable<ISkillDay> allSkillDays,
			ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, TimeZoneInfo timeZoneInfo, bool forRoleModel)
		{
			var bestShiftValue = double.MaxValue;
			IShiftProjectionCache ret =null;

			var skillDays = allSkillDays.FilterOnDate(datePointer);
			foreach (var shift in shifts)
			{
				var thisShiftsPeak = 0d;
				foreach (var layer in shift.MainShiftProjection)
				{
					var activity = (IActivity) layer.Payload;
					var thisShiftRequiresOneSeatExtra = activity.RequiresSeat;

					foreach (var skillDay in skillDays)
					{
						foreach (var interval in layer.Period.Intervals(TimeSpan.FromMinutes(skillDay.Skill.DefaultResolution)))
						{
							var skillStaffPeriod = skillDay.SkillStaffPeriodCollection.Single(x => x.Period == interval);
							var lackingSeatsThisInterval = _usedSeats.Fetch(skillStaffPeriod);// - skillStaffPeriod.Payload.MaxSeats; FIX this!
							if (thisShiftRequiresOneSeatExtra)
							{
								lackingSeatsThisInterval++;
							}
							thisShiftsPeak = Math.Max(thisShiftsPeak, lackingSeatsThisInterval);
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