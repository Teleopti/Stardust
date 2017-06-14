using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingProvider
	{
		private readonly IUserTimeZone _timeZone;
		private readonly INow _now;

		public ForecastedStaffingProvider(IUserTimeZone timeZone, INow now)
		{
			_timeZone = timeZone;
			_now = now;
		}
		
		public IList<StaffingIntervalModel> StaffingPerSkill(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, int minutesPerInterval, DateOnly? dateOnly, bool useShrinkage)
		{
			var startTimeLocal = dateOnly?.Date ?? TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).Date;
			var endTimeLocal = startTimeLocal.AddDays(1);

			var staffingIntervals = new List<StaffingIntervalModel>();
			var resolution = TimeSpan.FromMinutes(minutesPerInterval);
			foreach (var skill in skillDays.Keys)
			{
				if(skill is IChildSkill)
					continue;
				staffingIntervals.AddRange(skillDays[skill].SelectMany(x => getStaffingIntervalModels(x, resolution, useShrinkage)));
			}

			return staffingIntervals
				.Where(t => t.StartTime >= startTimeLocal && t.StartTime < endTimeLocal)
				.ToList();
		}

		private IEnumerable<StaffingIntervalModel> getStaffingIntervalModels(ISkillDay skillDay, TimeSpan resolution, bool useShrinkage)
		{
			var skillStaffPeriods = skillDay.SkillStaffPeriodViewCollection(resolution, useShrinkage);
	
			return skillStaffPeriods.Select(skillStaffPeriod => new StaffingIntervalModel
			{
				SkillId = skillDay.Skill.Id.Value,
				StartTime = TimeZoneHelper.ConvertFromUtc(skillStaffPeriod.Period.StartDateTime, _timeZone.TimeZone()),
				Agents = skillStaffPeriod.FStaff
			});
		}
	}
}