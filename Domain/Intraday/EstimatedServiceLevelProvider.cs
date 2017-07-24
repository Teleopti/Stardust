using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class EstimatedServiceLevelProvider
	{
		private readonly ForecastedCallsProvider _forecastedCallsProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IUserTimeZone _timeZone;

		public EstimatedServiceLevelProvider(ForecastedCallsProvider forecastedCallsProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			ScheduledStaffingProvider scheduledStaffingProvider,
			IUserTimeZone timeZone)
		{
			_forecastedCallsProvider = forecastedCallsProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_timeZone = timeZone;
		}

		public double EslSummary(IList<EslInterval> eslIntervals)
		{
			var sumOfForecastedCalls = eslIntervals.Sum(x => x.ForecastedCalls);
			var sumOfAnsweredCallsWithinSL = eslIntervals
				.Sum(x => x.AnsweredCallsWithinServiceLevel);
			return Math.Abs(sumOfForecastedCalls) > 0.01 ? sumOfAnsweredCallsWithinSL /sumOfForecastedCalls*100 ?? 0:0;
		}

		public IList<EslInterval> CalculateEslIntervals(
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, 
			IntradayIncomingViewModel queueStatistics,
			int minutesPerInterval)
		{
			var eslIntervals = new List<EslInterval>();
			if (!skillDays.Any())
				return eslIntervals;
			var serviceCalculatorService = new StaffingCalculatorServiceFacade();

			var forecastedCalls = _forecastedCallsProvider.Load(skillDays, queueStatistics.LatestActualIntervalStart, minutesPerInterval);
			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skillDays, minutesPerInterval, null, false)
				.Where(x => x.StartTime <= queueStatistics.LatestActualIntervalStart)
				.ToList();
			var scheduledStaffing = _scheduledStaffingProvider.StaffingPerSkill(skillDays.Keys.ToList(), minutesPerInterval);

			foreach (var skill in skillDays.Keys)
			{
				if(skill is IChildSkill)
					continue;
				var skillForecastedStaffing = forecastedStaffing
					.Where(s => s.SkillId == skill.Id.Value).ToList();
				foreach (var interval in skillForecastedStaffing)
				{
					eslIntervals.Add(calculateEslInterval(skillDays, 
						minutesPerInterval, interval, skill, scheduledStaffing, forecastedCalls, serviceCalculatorService));
				}
			}


			return eslIntervals
				.GroupBy(g => g.StartTime)
				.Select(s => new EslInterval
				{
					StartTime = s.Key,
					ForecastedCalls = s.Sum(x => x.ForecastedCalls),
					Esl = Math.Abs(s.Sum(x => x.ForecastedCalls)) < 0.01 ? null:s.Sum(x => x.AnsweredCallsWithinServiceLevel) / s.Sum(x => x.ForecastedCalls)
				})
				.OrderBy(t => t.StartTime)
				.ToList();
		}

		private EslInterval calculateEslInterval(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, 
			int minutesPerInterval, 
			StaffingIntervalModel interval,
			ISkill skill, 
			IList<SkillStaffingIntervalLightModel> scheduledStaffing, 
			ForecastedCallsModel forecastedCalls,
			StaffingCalculatorServiceFacade serviceCalculatorService)
		{
			var intervalStartTimeUtc = TimeZoneHelper.ConvertToUtc(interval.StartTime, _timeZone.TimeZone());
			var skillDaysForSkill = skillDays[skill];

			var serviceAgreement = getServiceAgreement(skillDaysForSkill, intervalStartTimeUtc);
			if (serviceAgreement == new ServiceAgreement())
				return new EslInterval();

			var scheduledStaffingInterval = scheduledStaffing
				.SingleOrDefault(x => x.Id == interval.SkillId && x.StartDateTime == interval.StartTime).StaffingLevel;
			var task = forecastedCalls.CallsPerSkill[skill.Id.Value].FirstOrDefault(x => x.StartTime == interval.StartTime);
			var esl = serviceCalculatorService.ServiceLevelAchievedOcc(
				scheduledStaffingInterval,
				serviceAgreement.ServiceLevel.Seconds,
				task.Calls,
				task.AverageHandleTime,
				TimeSpan.FromMinutes(minutesPerInterval),
				serviceAgreement.ServiceLevel.Percent.Value,
				interval.Agents,
				1);

			return new EslInterval
			{
				StartTime = interval.StartTime,
				ForecastedCalls = task.Calls,
				Esl = esl
			};
		}

		private static ServiceAgreement getServiceAgreement(IEnumerable<ISkillDay> skillDays, DateTime intervalStartTimeUtc)
		{
			ISkillData skillData = null;
			foreach (var skillDay in skillDays)
			{
				skillData = skillDay.SkillDataPeriodCollection
					.FirstOrDefault(
						skillDataPeriod => skillDataPeriod.Period.StartDateTime <= intervalStartTimeUtc &&
												 skillDataPeriod.Period.EndDateTime > intervalStartTimeUtc
					);
				if (skillData != null)
					break;
			}
			return skillData?.ServiceAgreement ?? new ServiceAgreement();
		}

		public double?[] DataSeries(IList<EslInterval> eslIntervals, IntradayIncomingViewModel queueIncoming, int minutesPerInterval)
		{
			var dataSeries = eslIntervals
				.Select(x => x.Esl * 100)
				.ToList();
		
			if (eslIntervals.Any())
			{
				var index = 0;
				var nullBeforeCount = (eslIntervals.Min(x => x.StartTime) - queueIncoming.DataSeries.Time.First()).TotalMinutes / minutesPerInterval;
				while (index < nullBeforeCount)
				{
					dataSeries.Insert(index, null);
					index = index + 1;
				}
			}
			
			var nullCount = queueIncoming.DataSeries.Time.Length - dataSeries.Count;
			dataSeries.AddRange(Enumerable.Repeat<double?>(null,nullCount));

			return dataSeries.ToArray();
		}
	}
}