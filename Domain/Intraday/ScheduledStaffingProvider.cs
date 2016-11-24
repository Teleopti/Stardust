using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ScheduledStaffingProvider
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly SkillStaffingIntervalProvider _skillStaffingIntervalProvider;


		public ScheduledStaffingProvider(INow now, 
			IUserTimeZone timeZone, 
			SkillStaffingIntervalProvider skillStaffingIntervalProvider)
		{
			_now = now;
			_timeZone = timeZone;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingPerSkill(IList<ISkill> skills, int minutesPerInterval)
		{
			var skillIdArray = skills.Select(x => x.Id.Value).ToArray();
			var startTimeLocal = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).Date;
			var endTimeLocal = startTimeLocal.AddDays(1);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(startTimeLocal, _timeZone.TimeZone()),
				TimeZoneHelper.ConvertToUtc(endTimeLocal, _timeZone.TimeZone()));
			var scheduledStaffing = _skillStaffingIntervalProvider.StaffingForSkills(skillIdArray, period, TimeSpan.FromMinutes(minutesPerInterval));

			return scheduledStaffing
				.Select(x => new SkillStaffingIntervalLightModel()
				{
					Id = x.Id,
					StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, _timeZone.TimeZone()),
					EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, _timeZone.TimeZone()),
					StaffingLevel = x.StaffingLevel
				})
				.ToList();
		}

		public double?[] DataSeries(IList<SkillStaffingIntervalLightModel> scheduledStaffing, DateTime[] timeSeries)
		{
			if (timeSeries.Length == scheduledStaffing.Count)
				return scheduledStaffing.Select(x => (double?)x.StaffingLevel).ToArray();

			List<double?> scheduledStaffingList = new List<double?>();
			foreach (var intervalStart in timeSeries)
			{
				var scheduledStaffingInterval = scheduledStaffing.FirstOrDefault(x => x.StartDateTime == intervalStart);
				scheduledStaffingList.Add(
					scheduledStaffingInterval.Equals(new SkillStaffingIntervalLightModel())
					? null
					: (double?)scheduledStaffingInterval.StaffingLevel);
			}
			return scheduledStaffingList.ToArray();
		}
	}
}