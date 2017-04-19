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
			var sourceTimeZone = _timeZone.TimeZone();
			var startTimeLocal = dateOnly?.Date ?? TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), sourceTimeZone).Date;
			var endTimeLocal = startTimeLocal.AddDays(1);

			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(startTimeLocal, sourceTimeZone),
				TimeZoneHelper.ConvertToUtc(endTimeLocal, sourceTimeZone));
			var scheduledStaffing = _skillStaffingIntervalProvider.StaffingForSkills(skillIdArray, period, TimeSpan.FromMinutes(minutesPerInterval), useShrinkage);

			return scheduledStaffing
				.Select(x => new SkillStaffingIntervalLightModel
				{
					Id = x.Id,
					StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, sourceTimeZone),
					EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, sourceTimeZone),
					StaffingLevel = x.StaffingLevel
				})
				.OrderBy(o => o.StartDateTime)
				.ToList();
		}
	}
}