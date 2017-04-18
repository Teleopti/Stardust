using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class SkillDayExtensions
	{
		public static ISkillDay CreateSkillDayWithDemandPerHour(this ISkill skill, IScenario scenario, DateOnly dateOnly, TimeSpan defaultDemand, Tuple<int, TimeSpan> specificHourDemand)
		{
		    return CreateSkillDayWithDemandPerHour(skill, scenario, dateOnly, defaultDemand,new List<Tuple<int, TimeSpan>>() {specificHourDemand});
		}

		public static ISkillDay CreateSkillDayWithDemandPerHour(this ISkill skill, IScenario scenario, DateOnly dateOnly, TimeSpan defaultDemand, List<Tuple<int, TimeSpan>> specificHourDemand)
		{
			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, skill.TimeZone);
			var skillDataPeriods = Enumerable.Range(0, 24).Select(hour =>
			{
				var period = new DateTimePeriod(dateTime.AddHours(hour), dateTime.AddHours(hour + 1));
				var demand = defaultDemand;
				var specificHour = specificHourDemand.Where(x => x.Item1 == hour);
				if (specificHour.Any())
				{
					demand = specificHour.First().Item2;
				}
				return new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(), period)
				{
					ManualAgents = demand.TotalHours
				};
			}).ToArray();

			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		public static ISkillDay CreateSkillDayWithDemandPerHour(this ISkill skill, IScenario scenario, DateOnly dateOnly, TimeSpan defaultDemand, Tuple<int, TimeSpan> specificHourDemand, TimePeriod openHours)
		{
			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, skill.TimeZone);
			var skillDataPeriods = Enumerable.Range((int)openHours.StartTime.TotalHours, (int)openHours.EndTime.TotalHours).Select(hour =>
			{
				var period = new DateTimePeriod(dateTime.AddHours(hour), dateTime.AddHours(hour + 1));
				var demand = specificHourDemand.Item1 == hour
					? specificHourDemand.Item2
					: defaultDemand;
				return new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(), period)
				{
					ManualAgents = demand.TotalHours
				};
			}).ToArray();

			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		public static ISkillDay CreateEmailSkillDayWithIncomingDemandOncePerDay(this ISkill skill, IScenario scenario, DateOnly dateOnly, TimeSpan defaultDemand, TimePeriod openHours)
		{
			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, skill.TimeZone);
			var skillDataPeriods = Enumerable.Range((int)openHours.StartTime.TotalHours, (int)openHours.EndTime.TotalHours).Select(hour =>
			{
				var period = new DateTimePeriod(dateTime.AddHours(hour), dateTime.AddHours(hour + 1));

				return
					new SkillDataPeriod(
						new ServiceAgreement(new ServiceLevel(new Percent(1), TimeSpan.FromHours(24).TotalSeconds), new Percent(0),
							new Percent(1)), new SkillPersonData(), period);
			}).ToArray();

			skillDataPeriods[0].ManualAgents = defaultDemand.TotalHours;
			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		private static ISkillDay setupSkillDay(ISkill skill, IScenario scenario, DateOnly dateOnly, IEnumerable<ISkillDataPeriod> skillDataPeriods)
		{
			var workloadDays = new List<IWorkloadDay>();
			var workloadDay = new WorkloadDay();
			var workload = skill.WorkloadCollection.First();
			workloadDay.CreateFromTemplate(dateOnly, workload,
				(IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, dateOnly.DayOfWeek));
			workloadDays.Add(workloadDay);
			var skillDay = new SkillDay(dateOnly, skill, scenario, workloadDays, skillDataPeriods);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { skillDay }, dateOnly.ToDateOnlyPeriod());
			return skillDay;
		}

		public static ISkillDay CreateSkillDayWithDemandOnInterval(this ISkill skill, IScenario scenario, DateOnly dateOnly,
			double defaultDemand, params Tuple<TimePeriod, double>[] intervalDemands)
		{
			var skillDataPeriods = new List<ISkillDataPeriod>();
			List<DateTimePeriod> intervals = dateOnly.ToDateTimePeriod(skill.TimeZone).Intervals(TimeSpan.FromMinutes(skill.DefaultResolution)).ToList();
			var midnightBreakDate = TimeZoneHelper.ConvertToUtc(dateOnly.Date.AddDays(1), TimeZoneInfo.Utc);
			var midnightBreakIntervals = new DateTimePeriod(midnightBreakDate,midnightBreakDate.Add(skill.MidnightBreakOffset))
									 .Intervals(TimeSpan.FromMinutes(skill.DefaultResolution)).ToList();
			intervals.AddRange(midnightBreakIntervals);
			var startDateTime = intervals.First().StartDateTime;
			var intervalDemandsDic = intervalDemands.ToDictionary(k => new DateTimePeriod(startDateTime.Add(k.Item1.StartTime), startDateTime.Add(k.Item1.EndTime)), v => v.Item2);
			foreach (var interval in intervals)
			{
				var intervalDemandMatch = intervalDemandsDic.SingleOrDefault(x => x.Key.Contains(interval));
				var demand = intervalDemandMatch.Key == new DateTimePeriod() ?
					defaultDemand :
					intervalDemandMatch.Value;
				skillDataPeriods.Add(new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(), interval) { ManualAgents = demand });
			}

			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		public static ISkillDay CreateSkillDayWithDemandOnIntervalWithSkillDataPeriodDuplicate(this ISkill skill, IScenario scenario, DateOnly dateOnly,
			double defaultDemand, params Tuple<TimePeriod, double>[] intervalDemands)
		{
			var skillDataPeriods = new List<ISkillDataPeriod>();
			var intervals = dateOnly.ToDateTimePeriod(skill.TimeZone).Intervals(TimeSpan.FromMinutes(skill.DefaultResolution));
			var startDateTime = intervals.First().StartDateTime;
			var intervalDemandsDic = intervalDemands.ToDictionary(k => new DateTimePeriod(startDateTime.Add(k.Item1.StartTime), startDateTime.Add(k.Item1.EndTime)), v => v.Item2);
			foreach (var interval in intervals)
			{
				var intervalDemandMatch = intervalDemandsDic.SingleOrDefault(x => x.Key.Contains(interval));
				var demand = intervalDemandMatch.Key == new DateTimePeriod() ?
					defaultDemand :
					intervalDemandMatch.Value;
				skillDataPeriods.Add(new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(), interval) { ManualAgents = demand });
			}
			if (intervals.Any())
			{
				skillDataPeriods.Add(new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(), skillDataPeriods.First().Period)
				{
					ManualAgents = skillDataPeriods.First().ManualAgents
				});
			}

			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		public static ISkillDay CreateSkillDayWithDemand(this ISkill skill, IScenario scenario, DateOnly dateOnly, TimeSpan demandPerInterval)
		{
			var manualAgents = (double)demandPerInterval.Ticks / TimeSpan.FromMinutes(skill.DefaultResolution).Ticks;
			return CreateSkillDayWithDemand(skill, scenario, dateOnly, manualAgents);
		}

		public static ISkillDay CreateSkillDayWithDemand(this ISkill skill, IScenario scenario, DateOnly dateOnly, double numberOfAgentsPerIntervalDemand)
		{
			var dateOnlyPeriod = dateOnly.ToDateOnlyPeriod();
			var skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
					dateOnlyPeriod.ToDateTimePeriod(skill.TimeZone))
				{ ManualAgents = numberOfAgentsPerIntervalDemand };

			var skillDataPeriods = new[] {skillDataPeriod};
			
			return setupSkillDay(skill, scenario, dateOnly, skillDataPeriods);
		}

		public static IList<ISkillDay> CreateSkillDayWithDemand(this ISkill skill, IScenario scenario, DateOnlyPeriod period, TimeSpan demandPerInterval)
		{
			return period.DayCollection()
				.Select(dateOnly => CreateSkillDayWithDemand(skill, scenario, dateOnly, demandPerInterval))
				.ToList();
		}

		public static IList<ISkillDay> CreateSkillDaysWithDemandOnConsecutiveDays(this ISkill skill, IScenario scenario, DateOnly startDate, params double[] numberOfAgentsPerIntervalDemand)
		{
			var skillDays = Enumerable.Range(0, numberOfAgentsPerIntervalDemand.Length).Select(day =>
			{
				var date = startDate.AddDays(day);
				return skill.CreateSkillDayWithDemand(scenario, date, numberOfAgentsPerIntervalDemand[day]);
			}).ToList();
			
			return skillDays;
		}
	}
}