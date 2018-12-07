using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class IntradayForecastingService
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorService;
		private readonly ISkillStaffingIntervalProvider _skillStaffingIntervalProvider;
		private readonly IIntradayStaffingService _staffingService;

		public IntradayForecastingService(
			IIntervalLengthFetcher intervalLengthFetcher,
			IStaffingCalculatorServiceFacade staffingCalculatorService,
			ISkillStaffingIntervalProvider skillStaffingIntervalProvider,
			IIntradayStaffingService staffingService
			)
		{
			_intervalLengthFetcher = intervalLengthFetcher ?? throw new ArgumentNullException(nameof(intervalLengthFetcher));
			_staffingCalculatorService = staffingCalculatorService ?? throw new ArgumentNullException(nameof(staffingCalculatorService));

			_skillStaffingIntervalProvider = skillStaffingIntervalProvider ?? throw new ArgumentNullException(nameof(skillStaffingIntervalProvider));
			_staffingService = staffingService ?? throw new ArgumentNullException(nameof(staffingService));
		}

		public IList<IntradayForecastInterval> GenerateForecast(IEnumerable<ISkillDay> skillDays, DateTime startTimeUtc, DateTime endTimeUtc, TimeSpan resolution, bool useShrinkage)
		{
			skillDays = skillDays.Where(x => !(x.Skill is ChildSkill));
			this.CalculateForecastedAgentsForEmailSkills(skillDays, startTimeUtc, endTimeUtc, useShrinkage);
			var periods = skillDays
				.SelectMany(x => x.SkillStaffPeriodViewCollection(resolution, useShrinkage).Select(i => new {SkillDay = x, StaffPeriod = i}));

			var forecast = periods
				.Where(x => x.StaffPeriod.Period.StartDateTime >= startTimeUtc && x.StaffPeriod.Period.EndDateTime <= endTimeUtc)
				.Select(x => new IntradayForecastInterval
				{
					SkillId = x.SkillDay.Skill.Id.Value,
					StartTime = x.StaffPeriod.Period.StartDateTime,
					Agents = x.StaffPeriod.FStaff,
					Calls = x.StaffPeriod.ForecastedTasks,
					AverageHandleTime = x.StaffPeriod.AverageHandlingTaskTime.TotalSeconds
				});

			return forecast.ToList();
		}

		public IList<EslInterval> CalculateEstimatedServiceLevels(IEnumerable<ISkillDay> skillDays, DateTime startOfPeriodUtc, DateTime endOfPeriodUtc)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var eslIntervals = new List<EslInterval>();

			if (!skillDays.Any())
				return eslIntervals;

			var mainSkillDays = skillDays.Where(x => !(x.Skill is IChildSkill) && x.Skill.Id.HasValue);
			
			var forecast = this.GenerateForecast(mainSkillDays, startOfPeriodUtc, endOfPeriodUtc, TimeSpan.FromMinutes(minutesPerInterval), false);

			if (!forecast.Any())
				return new List<EslInterval>();

			var skills = skillDays.GroupBy(x => x.Skill).Select(x => x.Key).ToList();
			var scheduledStaffing = _skillStaffingIntervalProvider
				.StaffingForSkills(skills.Select(x => x.Id.Value).ToArray(), new DateTimePeriod(startOfPeriodUtc, endOfPeriodUtc), TimeSpan.FromMinutes(minutesPerInterval), false)
				.Select(x =>
				{
					IChildSkill skill = (IChildSkill) skills.FirstOrDefault(c => c.Id.Equals(x.Id) && c.IsChildSkill);
					x.Id = (skill?.Id != null && skill is IChildSkill) ? skill.ParentSkill.Id.Value : x.Id;
					return (x);
				});

			var times = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(endOfPeriodUtc - startOfPeriodUtc).TotalMinutes / minutesPerInterval))
				.Select(offset => startOfPeriodUtc.AddMinutes(offset * minutesPerInterval));

			foreach (var skill in skills.Where(x => !(x is IChildSkill) && x.Id.HasValue))
			{
				var skillDataPeriods = skillDays.Where(x => x.Skill.Id == skill.Id).SelectMany(x => x.SkillDataPeriodCollection).OrderBy(x => x.Period.StartDateTime);
				var forecastForThisSkill = forecast.Where(f => f.SkillId == skill.Id);
				var skillData = times
					.Select(x =>
					{
						var forecastForInterval = forecastForThisSkill.Where(f => f.StartTime == x);
						if (forecastForInterval == null || !forecastForInterval.Any())
							return null;

						var sla = skillDataPeriods.FirstOrDefault(s => s.Period.StartDateTime <= x)?.ServiceAgreement;
						var forcastedCalls = forecastForInterval.Sum(f => f.Calls);
						return new
						{
							SkillId = skill.Id,
							StartTime = x,
							SkillAbandonRate = skill.AbandonRate.Value,
							SLA = sla,
							ScheduledStaffingLevel = scheduledStaffing.Where(s => s.Id.Equals(skill.Id) && s.StartDateTime.Equals(x)).Sum(s => s.StaffingLevel),
							ForecastedCalls = forcastedCalls,
							ForecastedAverageHandleTime = (forcastedCalls > 0) ? (forecastForInterval.Sum(f => f.AverageHandleTime * f.Calls) / forcastedCalls) : 0.0,
							ForecastedAgents = forecastForInterval.Sum(f => f.Agents)
						};
					});
				skillData = skillData.Where(x => x != null && x.SLA.HasValue);

				if (skillData.Any())
				{
					var eslIntervalForSkill = skillData
						.Select(x => new EslInterval
						{
							StartTime = x.StartTime,
							ForecastedCalls = x.ForecastedCalls,
							Esl = _staffingCalculatorService.ServiceLevelAchievedOcc(
								x.ScheduledStaffingLevel,
								x.SLA.Value.ServiceLevel.Seconds,
								x.ForecastedCalls,
								x.ForecastedAverageHandleTime,
								TimeSpan.FromMinutes(minutesPerInterval),
								x.SLA.Value.ServiceLevel.Percent.Value,
								x.ForecastedAgents,
								1,
								x.SkillAbandonRate)
						});

					eslIntervals.AddRange(eslIntervalForSkill);
				}
			}

			return eslIntervals
				.GroupBy(g => g.StartTime)
				.Select(s =>
				{
					var esls = s.Where(x => x.Esl.HasValue);
					var eslSum = esls.Any() ? (double ?) esls.Sum(x => x.Esl.Value) : null;
					var esl = (eslSum.HasValue && eslSum > 0.01) ? eslSum / esls.Count() : eslSum;

					return new EslInterval
					{
						StartTime = s.Key,
						ForecastedCalls = s.Sum(x => x.ForecastedCalls),
						Esl = esl
					};
				})
				.OrderBy(t => t.StartTime)
				.ToList();
		}
	
		
		public void CalculateForecastedAgentsForEmailSkills(IEnumerable<ISkillDay> skillDays, DateTime startDateUtc, DateTime endDateUtc, bool useShrinkage)
		{
			var skillDaysBySkill = skillDays.ToLookup(x => x.Skill);
			var skillGroupsByResuolution = skillDaysBySkill.Select(s => s.Key)
				.Where(SkillTypesWithBacklog.IsBacklogSkillType)
				.GroupBy(x => x.DefaultResolution);

			foreach (var group in skillGroupsByResuolution)
			{
				var emailSkillsForOneResoultion = group.ToList();
				var scheduledStaffingPerSkill = _staffingService
					.GetScheduledStaffing(
						emailSkillsForOneResoultion.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToArray(),
						startDateUtc.AddDays(-1),
						endDateUtc.AddDays(1),
						TimeSpan.FromMinutes(group.Key),
						useShrinkage).ToLookup(s => s.Id);
				foreach (var skill in emailSkillsForOneResoultion)
				{
					var skillDaysEmail = skillDaysBySkill[skill];
					foreach (var skillDay in skillDaysEmail)
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							var intervalStart = skillStaffPeriod.Period.StartDateTime;
							var scheduledStaff =
								scheduledStaffingPerSkill[skill.Id.Value].FirstOrDefault(x => x.StartDateTime == intervalStart);
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
