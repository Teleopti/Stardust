using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ForecastedStaffingProvider : IForecastedStaffingProvider
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
			var startTimeLocal = dateOnly?.Date ?? _now.CurrentLocalDateTime(_timeZone.TimeZone()).Date;
			var endTimeLocal = startTimeLocal.AddDays(1);

			var staffingIntervals = new List<StaffingInterval>();
			var resolution = TimeSpan.FromMinutes(minutesPerInterval);
			foreach (var skill in skillDays.Keys)
			{
				if(skill is IChildSkill)
					continue;
				staffingIntervals.AddRange(skillDays[skill].SelectMany(x => getStaffingIntervalModels(x, resolution, useShrinkage)));
			}

			return staffingIntervals
				.Where(t => t.StartTime >= startTimeLocal && t.StartTime < endTimeLocal)
				.Select(x => new StaffingIntervalModel
				{
					SkillId = x.SkillId,
					Agents = x.Agents,
					StartTime = x.StartTime
				})
				.ToList();
		}

		private IEnumerable<StaffingInterval> getStaffingIntervalModels(ISkillDay skillDay, TimeSpan resolution, bool useShrinkage)
		{
			var skillStaffPeriods = skillDay.SkillStaffPeriodViewCollection(resolution, useShrinkage);
	
			return skillStaffPeriods.Select(skillStaffPeriod => new StaffingInterval
			{
				SkillId = skillDay.Skill.Id.Value,
				StartTime = TimeZoneHelper.ConvertFromUtc(skillStaffPeriod.Period.StartDateTime, _timeZone.TimeZone()),
				Agents = skillStaffPeriod.FStaff
			});
		}
	}
}