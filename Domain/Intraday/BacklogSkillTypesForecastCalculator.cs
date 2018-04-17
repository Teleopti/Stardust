using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class BacklogSkillTypesForecastCalculator
	{
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;

		public BacklogSkillTypesForecastCalculator(ScheduledStaffingProvider scheduledStaffingProvider)
		{
			_scheduledStaffingProvider = scheduledStaffingProvider;
		}

		public void CalculateForecastedAgents(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, TimeZoneInfo timezone, bool useShrinkage)
		{
			var scheduledStaffingPerSkill = new List<SkillStaffingIntervalLightModel>();
			var skillGroupsByResuolution = skillDays.Keys
				.Where(SkillTypesWithBacklog.IsBacklogSkillType)
				.GroupBy(x => x.DefaultResolution);
			foreach (var group in skillGroupsByResuolution)
			{
				var emailSkillsForOneResoultion = group.ToList();

				foreach (var skill in emailSkillsForOneResoultion)
				{
					var skillDaysEmail = skillDays[skill];

					foreach (var skillDay in skillDaysEmail)
					{
						var skillDayDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(skillDay.CurrentDate.Date, timezone));
						scheduledStaffingPerSkill.AddRange(_scheduledStaffingProvider.StaffingPerSkill(emailSkillsForOneResoultion, group.Key, skillDayDate, useShrinkage));

						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							var intervalStartLocal = TimeZoneHelper.ConvertFromUtc(skillStaffPeriod.Period.StartDateTime, timezone);
							var scheduledStaff =
								scheduledStaffingPerSkill.FirstOrDefault(
									x => x.Id == skill.Id.Value && x.StartDateTime == intervalStartLocal);
							skillStaffPeriod.SetCalculatedResource65(0);
							if (scheduledStaff.StaffingLevel > 0)
								skillStaffPeriod.SetCalculatedResource65(scheduledStaff.StaffingLevel);
						}
					}
				}
			}
		}
	}
}