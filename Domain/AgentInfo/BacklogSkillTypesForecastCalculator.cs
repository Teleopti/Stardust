using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class BacklogSkillTypesForecastCalculator
	{
		private readonly IScheduledStaffingProvider _scheduledStaffingProvider;

		public BacklogSkillTypesForecastCalculator(IScheduledStaffingProvider scheduledStaffingProvider)
		{
			_scheduledStaffingProvider = scheduledStaffingProvider;
		}

		public void CalculateForecastedAgents(IDictionary<ISkill, IEnumerable<ISkillDay>> skillSkillDayDictionary, DateOnly dateOnly,
			TimeZoneInfo timezone, bool useShrinkage)
		{
			var scheduledStaffingPerSkill = new List<SkillStaffingIntervalLightModel>();
			var skillGroupsByResuolution = skillSkillDayDictionary.Keys
				.Where(SkillTypesWithBacklog.IsBacklogSkillType)
				.GroupBy(x => x.DefaultResolution);
			foreach (var group in skillGroupsByResuolution)
			{
				var emailSkillsForOneResoultion = group.ToList();
				scheduledStaffingPerSkill.AddRange(
					_scheduledStaffingProvider.StaffingPerSkill(emailSkillsForOneResoultion, group.Key, dateOnly, useShrinkage));

				foreach (var skill in emailSkillsForOneResoultion)
				{
					var skillDaysEmail = skillSkillDayDictionary[skill];
					foreach (var skillDay in skillDaysEmail)
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							var intervalStartLocal = skillStaffPeriod.Period.StartDateTimeLocal(timezone);
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