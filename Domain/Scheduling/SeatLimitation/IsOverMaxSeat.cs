using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class IsOverMaxSeat
	{
		private readonly IUsedSeats _usedSeats;

		public IsOverMaxSeat(IUsedSeats usedSeats)
		{
			_usedSeats = usedSeats;
		}

		public bool Check(IScheduleDay scheduleDay, IEnumerable<ISkillDay> maxSeatSkillDaysToLookAt)
		{
			foreach (var visualLayer in scheduleDay.ProjectionService().CreateProjection())
			{
				var affectedDates = new HashSet<DateOnly>
				{
					new DateOnly(visualLayer.Period.StartDateTime),
					new DateOnly(visualLayer.Period.EndDateTime)
				};
				foreach (var skillDay in maxSeatSkillDaysToLookAt.Where(x => affectedDates.Contains(x.CurrentDate)))
				{
					var maxSeats = ((MaxSeatSkill)skillDay.Skill).MaxSeats;

					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						if (visualLayer.Period.Intersect(skillStaffPeriod.Period))
						{
							if (_usedSeats.Fetch(skillStaffPeriod) - maxSeats > 0)
								return true;
						}
					}
				}
			}


			return false;
		}
	}
}