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

			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		private static ISkillDay setupSkillDay(ISkill skill, IScenario scenario, DateOnly dateOnly, IEnumerable<ISkillDataPeriod> skillDataPeriods)
		{
			var workloadDays = new List<IWorkloadDay>();
			var workloadDay = new WorkloadDay();
			var workload = skill.WorkloadCollection.First();
			workloadDay.CreateFromTemplate(dateOnly, workload,
				(IWorkloadDayTemplate) workload.GetTemplate(TemplateTarget.Workload, dateOnly.DayOfWeek));
			workloadDays.Add(workloadDay);
			var skillDay = new SkillDay(dateOnly, skill, scenario, workloadDays, skillDataPeriods);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> {skillDay},
				new DateOnlyPeriod(dateOnly, dateOnly));
			return skillDay;
		}

		public static ISkillDay CreateSkillDayWithDemandOnInterval(this ISkill skill, IScenario scenario, DateOnly dateOnly,
			double defaultDemand, params Tuple<TimePeriod, double>[] intervalDemands)
		{
			var skillDataPeriods = new List<ISkillDataPeriod>();
			var intervals = dateOnly.ToDateTimePeriod(skill.TimeZone).Intervals(TimeSpan.FromMinutes(skill.DefaultResolution));
			var startDateTime = intervals.First().StartDateTime;
			var intervalDemandsDic = intervalDemands.ToDictionary(k => new DateTimePeriod(startDateTime.Add(k.Item1.StartTime), startDateTime.Add(k.Item1.EndTime)), v => v.Item2);
			foreach (var interval in intervals)
			{
				double demand;
				if (!intervalDemandsDic.TryGetValue(interval, out demand))
				{
					demand = defaultDemand;
				}
				skillDataPeriods.Add(new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(), interval) { ManualAgents = demand });
			}

			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		public static ISkillDay CreateSkillDayWithDemand(this ISkill skill, IScenario scenario, DateOnly dateOnly, TimeSpan demandPerInterval)
		{
			var manualAgents = (double)demandPerInterval.Ticks/TimeSpan.FromMinutes(skill.DefaultResolution).Ticks;
			return CreateSkillDayWithDemand(skill, scenario, dateOnly, manualAgents);
		}

		public static ISkillDay CreateSkillDayWithDemand(this ISkill skill, IScenario scenario, DateOnly dateOnly, double numberOfAgentsPerIntervalDemand)
		{
			var skillDataPeriods = new List<ISkillDataPeriod>();
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
				dateOnlyPeriod.ToDateTimePeriod(skill.TimeZone))
			{ ManualAgents = numberOfAgentsPerIntervalDemand };

			skillDataPeriods.Add(skillDataPeriod);


			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		public static IList<ISkillDay> CreateSkillDayWithDemand(this ISkill skill, IScenario scenario, DateOnlyPeriod period, TimeSpan demandPerInterval)
		{
			return period.DayCollection()
				.Select(dateOnly => CreateSkillDayWithDemand(skill, scenario, dateOnly, demandPerInterval))
				.ToList();
		}

		public static Func<ISkillDay> CreateSkillDayWithDemandFactory(this ISkill skill, IScenario scenario, DateOnly date, TimeSpan demand)
		{
			return () => CreateSkillDayWithDemand(skill, scenario, date, demand);
		}

		public static IList<Func<ISkillDay>> CreateSkillDayWithDemandFactory(this ISkill skill, IScenario scenario, DateOnlyPeriod period, TimeSpan demand)
		{
			return period.DayCollection()
				.Select(day => CreateSkillDayWithDemandFactory(skill, scenario, day, demand))
				.ToList();
		}

		public static IList<ISkillDay> CreateSkillDaysWithDemandOnConsecutiveDays(this ISkill skill, IScenario scenario, DateOnly startDate, params double[] numberOfAgentsPerIntervalDemand)
		{
			var skillDays = new List<ISkillDay>();
			for (var day = 0; day < numberOfAgentsPerIntervalDemand.Length; day++)
			{
				var date = startDate.AddDays(day);
				var skillDay = skill.CreateSkillDayWithDemand(scenario, date, numberOfAgentsPerIntervalDemand[day]);
				skillDays.Add(skillDay);
			}
			return skillDays;
		}
	}
}