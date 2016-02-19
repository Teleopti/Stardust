using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class SkillExtensions
	{
		public static ISkillDay CreateSkillDayWithDemandPerHour(this ISkill skill, IScenario scenario, DateOnly dateOnly, TimeSpan defaultDemand, Tuple<int, TimeSpan> specificHourDemand)
		{
			var skillDataPeriods = new List<ISkillDataPeriod>();
			for (var hour = 0; hour < 24; hour++)
			{
				var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, skill.TimeZone);
				var period = new DateTimePeriod(dateTime.AddHours(hour), dateTime.AddHours(hour + 1));
				var demand = specificHourDemand.Item1 == hour ? 
					specificHourDemand.Item2 : 
					defaultDemand;
				skillDataPeriods.Add(new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),period) {ManualAgents = demand.TotalHours});

			}

			var workloadDays = new List<IWorkloadDay>();
			var workloadDay = new WorkloadDay();
			var workload = skill.WorkloadCollection.First();
			workloadDay.CreateFromTemplate(dateOnly, workload, (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, dateOnly.DayOfWeek));
			workloadDays.Add(workloadDay);

			var skillDay = new SkillDay(dateOnly, skill, scenario, workloadDays, skillDataPeriods);

			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { skillDay }, new DateOnlyPeriod(dateOnly, dateOnly));
			return skillDay;
		}


		public static ISkillDay CreateSkillDayWithDemand(this ISkill skill, IScenario scenario, DateOnly dateOnly, TimeSpan demand)
		{
			var skillDataPeriods = new List<ISkillDataPeriod>();
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
				dateOnlyPeriod.ToDateTimePeriod(skill.TimeZone))
			{ ManualAgents = demand.TotalHours };

			skillDataPeriods.Add(skillDataPeriod);

			var workloadDays = new List<IWorkloadDay>();
			var workloadDay = new WorkloadDay();
			var workload = skill.WorkloadCollection.First();
			workloadDay.CreateFromTemplate(dateOnly, workload, (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, dateOnly.DayOfWeek));
			workloadDays.Add(workloadDay);

			var skillDay = new SkillDay(dateOnly, skill, scenario, workloadDays, skillDataPeriods);

			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { skillDay }, dateOnlyPeriod);
			return skillDay;
		}


		public static IList<ISkillDay> CreateSkillDayWithDemand(this ISkill skill, IScenario scenario, DateOnlyPeriod period, TimeSpan demand)
		{
			return period.DayCollection()
				.Select(dateOnly => CreateSkillDayWithDemand(skill, scenario, dateOnly, demand))
				.ToList();
		}

		public static IList<ISkillDay> CreateSkillDaysWithDemandOnConsecutiveDays(this ISkill skill, IScenario scenario, DateOnly startDate, params TimeSpan[] demands)
		{
			var skillDays = new List<ISkillDay>();
			for (var day = 0; day < demands.Length; day++)
			{
				var date = startDate.AddDays(day);
				var skillDay = skill.CreateSkillDayWithDemand(scenario, date, demands[day]);
				skillDays.Add(skillDay);
			}
			return skillDays;
		}
	}
}