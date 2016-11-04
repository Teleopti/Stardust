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
			var maxSeatSkillDays = skillDays.Where(x => x.Skill is MaxSeatSkill);
			var normalSkillDays = skillDays.Except(maxSeatSkillDays);

			foreach (var shift in shifts)
			{
				var thisShiftsPeak = 0d;

				foreach (var layer in shift.MainShiftProjection)
				{
					var activity = (IActivity) layer.Payload;
					var thisShiftRequiresOneSeatExtra = activity.RequiresSeat;

					if (!checkSkillIsOpen(normalSkillDays, layer))
					{
						thisShiftsPeak = double.MaxValue;
						break;
					}

					foreach (var skillDay in maxSeatSkillDays)
					{
						foreach (var interval in layer.Period.Intervals(TimeSpan.FromMinutes(skillDay.Skill.DefaultResolution)))
						{
							var skillStaffPeriod = skillDay.SkillStaffPeriodCollection.Single(x => x.Period == interval);
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

		//gör en klass av detta
		private static bool checkSkillIsOpen(IEnumerable<ISkillDay> normalSkillDays, IVisualLayer layer)
		{
			if (!normalSkillDays.Any())
				return true;

			foreach (var skillDay in normalSkillDays)
			{
				if (skillDay.Skill.Activity.Equals((IActivity)layer.Payload))
				{
					//not correct
					var open = skillDay.SkillStaffPeriodCollection.First().Period.StartDateTime;
					var close = skillDay.SkillDataPeriodCollection.Last().Period.EndDateTime;


					if (new DateTimePeriod(open, close).Contains(layer.Period)) 
						return true;

				}
			}
			return false;
		}
	}
}