using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class BacklogSkillTypesForecastCalculator
	{
		private readonly IScheduledStaffingProvider _scheduledStaffingProvider;

		public BacklogSkillTypesForecastCalculator(IScheduledStaffingProvider scheduledStaffingProvider)
		{
			_scheduledStaffingProvider = scheduledStaffingProvider;
		}

		public void CalculateForecastedAgents(IDictionary<ISkill, IEnumerable<ISkillDay>> skillSkillDayDictionary, DateOnly dateOnly, TimeZoneInfo timezone, bool useShrinkage)
		{
			var skillGroupsByResolution = skillSkillDayDictionary.Keys
				.Where(SkillTypesWithBacklog.IsBacklogSkillType)
				.GroupBy(x => x.DefaultResolution);
			foreach (var group in skillGroupsByResolution)
			{
				var emailSkillsForOneResolution = group.ToArray();
				var skillStaffingIntervalLightModels = _scheduledStaffingProvider.StaffingPerSkill(emailSkillsForOneResolution, @group.Key, dateOnly, useShrinkage);
				var staffingBySkill = skillStaffingIntervalLightModels.ToLookup(s => (s.Id, s.StartDateTime));

				foreach (var skill in emailSkillsForOneResolution)
				{
					var skillDaysEmail = skillSkillDayDictionary[skill];
					foreach (var skillDay in skillDaysEmail)
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							var intervalStartLocal = skillStaffPeriod.Period.StartDateTimeLocal(timezone);
							var scheduledStaff = staffingBySkill[(skill.Id.Value, intervalStartLocal)].FirstOrDefault();
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