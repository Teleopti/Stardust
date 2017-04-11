using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ScheduledStaffingProvider
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly ISkillStaffingIntervalProvider _skillStaffingIntervalProvider;


		public ScheduledStaffingProvider(INow now, 
			IUserTimeZone timeZone, 
			ISkillStaffingIntervalProvider skillStaffingIntervalProvider)
		{
			_now = now;
			_timeZone = timeZone;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingPerSkill(IList<ISkill> skills, int minutesPerInterval, DateOnly? dateOnly = null, bool useShrinkage = false)
		{
			var skillIdArray = skills.Select(x => x.Id.Value).ToArray();
			var startTimeLocal = dateOnly?.Date ?? TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).Date;
			var endTimeLocal = startTimeLocal.AddDays(1);

			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(startTimeLocal, _timeZone.TimeZone()),
				TimeZoneHelper.ConvertToUtc(endTimeLocal, _timeZone.TimeZone()));
			var scheduledStaffing = _skillStaffingIntervalProvider.StaffingForSkills(skillIdArray, period, TimeSpan.FromMinutes(minutesPerInterval), useShrinkage);

			return scheduledStaffing
				.Select(x => new SkillStaffingIntervalLightModel()
				{
					Id = x.Id,
					StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, _timeZone.TimeZone()),
					EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, _timeZone.TimeZone()),
					StaffingLevel = x.StaffingLevel
				})
				.OrderBy(o => o.StartDateTime)
				.ToList();
		}

		public double?[] DataSeries(IList<SkillStaffingIntervalLightModel> scheduledStaffing, DateTime[] timeSeries)
		{
			if (!scheduledStaffing.Any())
				return new double?[] {};

			var staffingIntervals = scheduledStaffing
				.GroupBy(x => x.StartDateTime)
				.Select(s => new StaffingStartInterval
				{
					StartTime = s.Key,
					StaffingLevel = s.Sum(a => a.StaffingLevel)
				})
				.ToList();

			if (timeSeries.Length == staffingIntervals.Count)
				return staffingIntervals.Select(x => (double?)x.StaffingLevel).ToArray();

			List<double?> scheduledStaffingList = new List<double?>();
			foreach (var intervalStart in timeSeries)
			{
				var scheduledStaffingInterval = staffingIntervals.FirstOrDefault(x => x.StartTime == intervalStart);
				scheduledStaffingList.Add(scheduledStaffingInterval?.StaffingLevel);
			}
			return scheduledStaffingList.ToArray();
		}

		private class StaffingStartInterval
		{
			public DateTime StartTime { get; set; }
			public double StaffingLevel { get; set; }
		}
	}
}