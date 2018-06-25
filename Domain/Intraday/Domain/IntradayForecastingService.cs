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
		IList<StaffingIntervalModel> GetForecastedStaffing(
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
		void CalculateForecastedAgentsForEmailSkills(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, bool useShrinkage);
	}

	public class IntradayForecastingService : IIntradayForecastingService
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorService;
		private readonly SkillStaffingIntervalProvider _skillStaffingIntervalProvider;
		private readonly IIntradayStaffingService _staffingService;

		public IntradayForecastingService(
			IIntervalLengthFetcher intervalLengthFetcher,
			IStaffingCalculatorServiceFacade staffingCalculatorService,
			SkillStaffingIntervalProvider skillStaffingIntervalProvider,
			IIntradayStaffingService staffingService
			)
		{
			_intervalLengthFetcher = intervalLengthFetcher ?? throw new ArgumentNullException(nameof(intervalLengthFetcher));
			_staffingCalculatorService = staffingCalculatorService ?? throw new ArgumentNullException(nameof(staffingCalculatorService));

			_skillStaffingIntervalProvider = skillStaffingIntervalProvider ?? throw new ArgumentNullException(nameof(skillStaffingIntervalProvider));
			_staffingService = staffingService ?? throw new ArgumentNullException(nameof(staffingService));
		}

		public IList<StaffingIntervalModel> GetForecastedStaffing(
			IEnumerable<ISkillDay> skillDays, 
			DateTime startTimeUtc, 
			DateTime endTimeUtc,
			TimeSpan resolution,
			bool useShrinkage)
		{
			var staffingIntervals = skillDays
				.Where(x => !(x.Skill is IChildSkill) && x.Skill.Id.HasValue)
				.SelectMany(x =>
				{
					var skillStaffPeriods = x.SkillStaffPeriodViewCollection(resolution, useShrinkage);

					return skillStaffPeriods
						.Where(t => t.Period.StartDateTime >= startTimeUtc && t.Period.StartDateTime < endTimeUtc)
						.Select(skillStaffPeriod => new StaffingIntervalModel
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

			if (skillDays == null || !skillDays.Any())
				return eslIntervals;

			skillDays = skillDays.Where(x => !(x.Skill is IChildSkill) && x.Skill.Id.HasValue);
			var skills = skillDays.GroupBy(x => x.Skill).Select(x => x.Key).ToList();
			var forcastedVolumes = GetForecastedCalls(skillDays, startOfPeriodUtc, endOfPeriodUtc);
			var forecastedStaffing = GetForecastedStaffing(skillDays, startOfPeriodUtc, endOfPeriodUtc, TimeSpan.FromMinutes(intervalLength), false)
				.Where(x => x.StartTime >= startOfPeriodUtc && x.StartTime <= endOfPeriodUtc);

			if (!forcastedVolumes.Any() || !forecastedStaffing.Any())
				return new List<EslInterval>();

			var period = new DateTimePeriod(startOfPeriodUtc, endOfPeriodUtc);
			var scheduledStaffing = _skillStaffingIntervalProvider
				.StaffingForSkills(skills.Select(x => x.Id.Value).ToArray(), period, TimeSpan.FromMinutes(intervalLength), false);

			var scheduledStaffingPerSkill = scheduledStaffing
				.GroupBy(x => new { SkillId = x.Id, x.StartDateTime })
				.Select(x => new
				{
					x.Key.SkillId,
					x.Key.StartDateTime,
					StaffingLevel = x.Sum(y => y.StaffingLevel)
				})
				.OrderBy(o => o.StartDateTime);

			for (var iTime = startOfPeriodUtc; iTime < endOfPeriodUtc; iTime = iTime.AddMinutes(intervalLength))
			{
				foreach (var skill in skills)
				{
					var forecastedVolume = forcastedVolumes.Where(x => x.StartTime == iTime && x.SkillId == skill.Id);
					var skillData = skillDays
						.Where(x => x.Skill.Id == skill.Id)
						.SelectMany(x => x.SkillDataPeriodCollection.Where(d => d.Period.StartDateTime <= iTime && d.Period.EndDateTime > iTime))
						.FirstOrDefault();

					if (forecastedVolume == null || skillData?.ServiceAgreement == null)
						continue;

					var esl = _staffingCalculatorService.ServiceLevelAchievedOcc(
						scheduledStaffingPerSkill.Where(x => x.StartDateTime == iTime && x.SkillId == skill.Id).Sum(x => x.StaffingLevel),
						skillData.ServiceAgreement.ServiceLevel.Seconds,
						forecastedVolume.Sum(x => x.Calls),
						forecastedVolume.Sum(x => x.AverageHandleTime),
						TimeSpan.FromMinutes(intervalLength),
						skillData.ServiceAgreement.ServiceLevel.Percent.Value,
						forecastedStaffing.Where(x => x.StartTime == iTime && x.SkillId == skill.Id).Sum(x => x.Agents),
						1,
						skill.AbandonRate.Value);

					eslIntervals.Add(new EslInterval
					{
						StartTime = iTime,
						ForecastedCalls = forecastedVolume.Sum(x => x.Calls),
						Esl = esl
					});
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
	
		
		public void CalculateForecastedAgentsForEmailSkills(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, bool useShrinkage)
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
