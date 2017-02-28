﻿using System;
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
		public IList<StaffingIntervalModel> StaffingPerSkill(IList<ISkill> skills, ICollection<ISkillDay> skillDays, int minutesPerInterval)
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);
			var staffingIntervals = new List<StaffingIntervalModel>();
			var resolution = TimeSpan.FromMinutes(minutesPerInterval);
			foreach (var skill in skills)
			{
				var skillDaysForSkill = skillDays.Where(x => x.Skill.Id.Value == skill.Id.Value).ToList();
				foreach (var skillDay in skillDaysForSkill)
				{
					staffingIntervals.AddRange(getStaffingIntervalModels(skillDay, resolution));
				}
			}

			return staffingIntervals
				.Where(t => t.StartTime >= usersToday.Date && t.StartTime < usersToday.Date.AddDays(1))
				.ToList();
		}

		private IEnumerable<StaffingIntervalModel> getStaffingIntervalModels(ISkillDay skillDay, TimeSpan resolution)
		{
			var skillStaffPeriods = skillDay.SkillStaffPeriodViewCollection(resolution);

			return skillStaffPeriods.Select(skillStaffPeriod => new StaffingIntervalModel
			{
				SkillId = skillDay.Skill.Id.Value,
				StartTime = TimeZoneHelper.ConvertFromUtc(skillStaffPeriod.Period.StartDateTime, _timeZone.TimeZone()),
				Agents = skillStaffPeriod.FStaff
			});
		}

		public double?[] DataSeries(IList<StaffingIntervalModel> forecastedStaffingPerSkill, DateTime[] timeSeries)
		{
			var forecastedStaffing = forecastedStaffingPerSkill
				.GroupBy(g => g.StartTime)
				.Select(s =>
				{
					var staffingIntervalModel = s.First();
					return new StaffingIntervalModel
					{
						SkillId = staffingIntervalModel.SkillId,
						StartTime = staffingIntervalModel.StartTime,
						Agents = s.Sum(a => a.Agents)
					};
				})
				.OrderBy(o => o.StartTime)
				.ToList();

			if (timeSeries.Length == forecastedStaffing.Count)
				return forecastedStaffing.Select(x => (double?)x.Agents).ToArray();

			List<double?> forecastedStaffingList = new List<double?>();
			foreach (var intervalStart in timeSeries)
			{
				var forecastedStaffingInterval = forecastedStaffing.FirstOrDefault(x => x.StartTime == intervalStart);
				forecastedStaffingList.Add(forecastedStaffingInterval?.Agents);
			}
			return forecastedStaffingList.ToArray();
		}
	}
}