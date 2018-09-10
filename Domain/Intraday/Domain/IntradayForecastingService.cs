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
		IList<StaffingInterval> GetForecastedStaffing(
			IEnumerable<ISkillDay> skillDays, 
			DateTime startTimeUtc,
			DateTime endTimeUtc,
			TimeSpan resolution,
			bool useShrinkage);

		IEnumerable<SkillIntervalStatistics> GetForecastedCalls(
			IEnumerable<ISkillDay> skillDays,
			DateTime startAtUtc,
			DateTime endAtUtc);

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

		public IList<StaffingInterval> GetForecastedStaffing(
			IEnumerable<ISkillDay> skillDays, 
			DateTime startTimeUtc, 
			DateTime endTimeUtc,
			TimeSpan resolution,
			bool useShrinkage)
		{
			this.CalculateForecastedAgentsForEmailSkills(skillDays, useShrinkage);
			var staffingIntervals = skillDays
				.Where(x => !(x.Skill is IChildSkill) && x.Skill.Id.HasValue)
				.SelectMany(x =>
				{
					var skillStaffPeriods = x.SkillStaffPeriodViewCollection(resolution, useShrinkage);

					return skillStaffPeriods
						.Where(t => t.Period.StartDateTime >= startTimeUtc && t.Period.StartDateTime < endTimeUtc)
						.Select(skillStaffPeriod => new StaffingInterval
						{
							SkillId = x.Skill.Id.Value,
							StartTime = skillStaffPeriod.Period.StartDateTime,
							Agents = skillStaffPeriod.FStaff
						});
				});

			return staffingIntervals.ToList();
		}

		public IEnumerable<SkillIntervalStatistics> GetForecastedCalls(
			IEnumerable<ISkillDay> skillDays, 
			DateTime startAtUtc, 
			DateTime endAtUtc)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;

			var allForecastedCalls = new List<SkillIntervalStatistics>();
			foreach (var skillDay in skillDays)
			{
				var templateTaskPeriods = GetTemplateTaskPeriods(skillDay, minutesPerInterval, startAtUtc, endAtUtc);
				var forecastedCalls = templateTaskPeriods
					.GroupBy(x => x.Period.StartDateTime)
					.OrderBy(x => x.Key)
					.Select(x => new SkillIntervalStatistics
					{
						SkillId = skillDay.Skill.Id.Value,
						StartTime = x.Key,
						Calls = x.Sum(s => s.TotalTasks),
						AverageHandleTime = x.Sum(s => s.AverageTaskTime.TotalSeconds + s.AverageAfterTaskTime.TotalSeconds)
					});
				allForecastedCalls.AddRange(forecastedCalls);
			}
			return allForecastedCalls;
		}

		public IList<EslInterval> CalculateEstimatedServiceLevels(IEnumerable<ISkillDay> skillDays, DateTime startOfPeriodUtc, DateTime endOfPeriodUtc)
		{
			var intervalLength = _intervalLengthFetcher.IntervalLength;
			var eslIntervals = new List<EslInterval>();

			if (!skillDays.Any())
				return eslIntervals;

			var skills = skillDays
				.GroupBy(x => x.Skill)
				.Select(x => x.Key)
				.ToList();

			var mainSkillDays = skillDays.Where(x => !(x.Skill is IChildSkill) && x.Skill.Id.HasValue);

			var forcastedVolumes = this.GetForecastedCalls(mainSkillDays, startOfPeriodUtc, endOfPeriodUtc);
			var forecastedStaffing = this.GetForecastedStaffing(
				mainSkillDays, 
				startOfPeriodUtc, 
				endOfPeriodUtc, 
				TimeSpan.FromMinutes(intervalLength),
				false);

			if (!forcastedVolumes.Any() || !forecastedStaffing.Any())
				return new List<EslInterval>();

			var scheduledStaffing = _skillStaffingIntervalProvider
				.StaffingForSkills(skills.Select(x => x.Id.Value).ToArray(), new DateTimePeriod(startOfPeriodUtc, endOfPeriodUtc), TimeSpan.FromMinutes(intervalLength), false)
				.Select(x =>
				{
					IChildSkill skill = (IChildSkill) skills.FirstOrDefault(c => c.Id.Equals(x.Id) && c.IsChildSkill);
					x.Id = (skill?.Id != null && skill is IChildSkill) ? skill.ParentSkill.Id.Value : x.Id;
					return (x);
				});

			var times = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(endOfPeriodUtc - startOfPeriodUtc).TotalMinutes / intervalLength))
				.Select(offset => startOfPeriodUtc.AddMinutes(offset * intervalLength));

			foreach (var skill in skills.Where(x => !(x is IChildSkill) && x.Id.HasValue))
			{
				var skillDataPeriods = skillDays.Where(x => x.Skill.Id == skill.Id).SelectMany(x => x.SkillDataPeriodCollection).OrderBy(x => x.Period.StartDateTime);
				var forecastedVolumesForThisSkill = forcastedVolumes.Where(f => f.SkillId == skill.Id);
				var skillData = times
					.Select(x =>
					{
						var forecastedVolume = forecastedVolumesForThisSkill.Where(f => f.StartTime == x);
						if (forecastedVolume == null || !forecastedVolume.Any())
							return null;

						var forecastedStaffingForThisSkill = forecastedStaffing.Where(f => f.SkillId == skill.Id);
						var sla = skillDataPeriods.FirstOrDefault(s => s.Period.StartDateTime <= x)?.ServiceAgreement;
						return new
						{
							SkillId = skill.Id,
							StartTime = x,
							SkillAbandonRate = skill.AbandonRate.Value,
							SLA = sla,
							ScheduledStaffingLevel = scheduledStaffing.Where(s => s.StartDateTime.Equals(x)).Sum(s => s.StaffingLevel),
							ForecastedCalls = forecastedVolume.Sum(f => f.Calls),
							ForecastedAverageHandleTime = forecastedVolume.Sum(f => f.AverageHandleTime),
							ForecastedAgents = forecastedStaffingForThisSkill.Where(f => f.StartTime == x).Sum(f => f.Agents)
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
								TimeSpan.FromMinutes(intervalLength),
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

		public IEnumerable<ITemplateTaskPeriod> GetTemplateTaskPeriods(ISkillDay skillDay, int minutesPerInterval, DateTime startOfPeriodUtc, DateTime endOfPeriodUtc)
		{
			if (startOfPeriodUtc == null || endOfPeriodUtc == null)
			{
				return Enumerable.Empty<ITemplateTaskPeriod>();
			}

			var taskPeriods = new List<ITemplateTaskPeriod>();
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				IList<ITemplateTaskPeriod> templateTaskPeriodCollection = workloadDay.OpenTaskPeriodList;
				if (workloadDay.OpenTaskPeriodList.Any() && minutesPerInterval <= skillDay.Skill.DefaultResolution)
				{
					if (minutesPerInterval < skillDay.Skill.DefaultResolution || IsTaskPeriodsMerged(templateTaskPeriodCollection, skillDay.Skill.DefaultResolution))
						templateTaskPeriodCollection = SplitTaskPeriods(templateTaskPeriodCollection, TimeSpan.FromMinutes(minutesPerInterval));

					var temp = templateTaskPeriodCollection.Where(t =>
						t.Period.StartDateTime >= startOfPeriodUtc && t.Period.StartDateTime < endOfPeriodUtc);
					taskPeriods.AddRange(temp);
				}
			}
			return taskPeriods;
		}

		public bool IsTaskPeriodsMerged(IList<ITemplateTaskPeriod> taskPeriodCollection, int skillResolution)
		{
			var periodStart = taskPeriodCollection.Min(x => x.Period.StartDateTime);
			var periodEnd = taskPeriodCollection.Max(x => x.Period.EndDateTime);
			var periodLength = (int)periodEnd.Subtract(periodStart).TotalMinutes;
			var expectedIntervalCount = periodLength / skillResolution;
			return (expectedIntervalCount != taskPeriodCollection.Count);
		}

		public IList<ITemplateTaskPeriod> SplitTaskPeriods(IList<ITemplateTaskPeriod> templateTaskPeriodCollection, TimeSpan periodLength)
		{
			List<ITemplateTaskPeriod> returnList = new List<ITemplateTaskPeriod>();
			foreach (var taskPeriod in templateTaskPeriodCollection)
			{
				var splittedTaskPeriods = taskPeriod.Split(periodLength);
				returnList.AddRange(splittedTaskPeriods.Select(p => new TemplateTaskPeriod(
					new Task(p.TotalTasks, p.TotalAverageTaskTime, p.TotalAverageAfterTaskTime), p.Period)));
			}
			return returnList;
		}
	}
}
