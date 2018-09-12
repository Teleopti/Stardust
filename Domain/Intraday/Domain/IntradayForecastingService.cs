using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public interface IIntradayForecastingService
	{
		IList<IntradayForecastInterval> GenerateForecast(IEnumerable<ISkillDay> skillDays, DateTime startTimeUtc, DateTime endTimeUtc, TimeSpan resolution, bool useShrinkage);
		IList<EslInterval> CalculateEstimatedServiceLevels(IEnumerable<ISkillDay> skillDays, DateTime startOfPeriodUtc, DateTime endOfPeriodUtc);
	}

	public class IntradayForecastingService : IIntradayForecastingService
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
			this.CalculateForecastedAgentsForEmailSkills(skillDays, useShrinkage);
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
			var useShrinkage = false;
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var eslIntervals = new List<EslInterval>();

			if (!skillDays.Any())
				return eslIntervals;


			this.CalculateForecastedAgentsForEmailSkills(skillDays, useShrinkage);

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
						return new
						{
							SkillId = skill.Id,
							StartTime = x,
							SkillAbandonRate = skill.AbandonRate.Value,
							SLA = sla,
							ScheduledStaffingLevel = scheduledStaffing.Where(s => s.StartDateTime.Equals(x)).Sum(s => s.StaffingLevel),
							ForecastedCalls = forecastForInterval.Sum(f => f.Calls),
							ForecastedAverageHandleTime = forecastForInterval.Sum(f => f.AverageHandleTime),
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
				.Select(s => new EslInterval
				{
					StartTime = s.Key,
					ForecastedCalls = s.Sum(x => x.ForecastedCalls),
					Esl = Math.Abs(s.Sum(x => x.ForecastedCalls)) < 0.01 ? null : s.Sum(x => x.AnsweredCallsWithinServiceLevel) / s.Sum(x => x.ForecastedCalls)
				})
				.OrderBy(t => t.StartTime)
				.ToList();
		}
	
		
		public void CalculateForecastedAgentsForEmailSkills(IEnumerable<ISkillDay> skillDays, bool useShrinkage)
		{
			var skills = skillDays.Select(x => x.Skill).Distinct();

			var scheduledStaffingPerSkill = new List<SkillStaffingIntervalLightModel>();

			var skillGroupsByResuolution = skills
				.Where(SkillTypesWithBacklog.IsBacklogSkillType)
				.GroupBy(x => x.DefaultResolution);

			foreach (var group in skillGroupsByResuolution)
			{
				var emailSkillsForOneResoultion = group.ToList();

				foreach (var skill in emailSkillsForOneResoultion)
				{
					var skillDaysEmail = skillDays.Where(x => x.Skill.Equals(skill));
					foreach (var skillDay in skillDaysEmail)
					{
						var skillDayStartUtc = DateTime.SpecifyKind(skillDay.CurrentDate.Date, DateTimeKind.Utc);
						scheduledStaffingPerSkill.AddRange(_staffingService
							.GetScheduledStaffing(
								emailSkillsForOneResoultion.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToArray(), 
								skillDayStartUtc,
								skillDayStartUtc.AddDays(1), 
								TimeSpan.FromMinutes(group.Key), 
								useShrinkage));

						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							var intervalStartLocal = skillStaffPeriod.Period.StartDateTime;
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
