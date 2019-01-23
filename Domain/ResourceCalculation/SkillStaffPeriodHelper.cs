using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Helper functions for working with <see cref="ISkillStaffPeriod"/> 
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-02-13
    /// </remarks>
    public static class SkillStaffPeriodHelper
    {
        public static IList<double> SkillStaffPeriodsAbsoluteDifferenceHours(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing)
        {
            if(considerMinStaffing && considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.AbsoluteDifferenceBoosted() * s.DateTimePeriod.ElapsedTime().TotalHours).ToList();
            if(considerMinStaffing)
                return skillStaffPeriods.Select(s => s.AbsoluteDifferenceMinStaffBoosted() * s.DateTimePeriod.ElapsedTime().TotalHours).ToList();
            if (considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.AbsoluteDifferenceMaxStaffBoosted() * s.DateTimePeriod.ElapsedTime().TotalHours).ToList();
            return skillStaffPeriods.Select(s => s.AbsoluteDifference * s.DateTimePeriod.ElapsedTime().TotalHours).ToList();
        }

        /// <summary>
        /// Gets the intraday relative differences in hours between forecasted and scheduled resources in the specified
        /// <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <param name="considerMinStaffing">if set to <c>true</c> then consider the preset minimum staffing.</param>
        /// <param name="considerMaxStaffing">if set to <c>true</c> then consider the preset maximum staffing.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the difference diference in hours for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static IList<double> SkillStaffPeriodsRelativeDifferenceHours(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing)
        {
            if (considerMinStaffing && considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            if (considerMinStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceMinStaffBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            if (considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceMaxStaffBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            return skillStaffPeriods.Select(s => s.RelativeDifference * s.Period.ElapsedTime().TotalHours).ToList();
        }

		/// <summary>
		/// Gets max of used seats during the period
		/// </summary>
		/// <param name="skillStaffPeriods">The skill staff periods.</param>
		/// <returns></returns>
		public static double? MaxUsedSeats(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
		{
			var list = skillStaffPeriods.Select(s => s.Payload.CalculatedUsedSeats).ToList();
			return (list.Count > 0) ? list.Max() : (double?)null;
		}

        /// <summary>
        /// Gets the absolute difference time between forecasted and scheduled in the specified <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the absolute difference time for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static TimeSpan? AbsoluteDifference(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            var list = SkillStaffPeriodsAbsoluteDifferenceHours(skillStaffPeriods, false, false);
            if (list.Count == 0)
                return null;
            return TimeSpan.FromHours(list.Sum());
        }

        /// <summary>
        /// Gets the Forecasted time in the specified <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the forecasted time for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static TimeSpan? ForecastedTime(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            TimeSpan? ret = null;
            
            foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriods)
            {
            	var toAdd = skillStaffPeriod.FStaffTime();
				ret = !ret.HasValue ? toAdd : ret.Value.Add(toAdd);
            }

            return ret;
        }

        /// <summary>
        /// Gets the Scheduled hours in the specified <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the scheduled hours for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static double? ScheduledHours(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            double? ret = null;
            TimeSpan? time = ScheduledTime(skillStaffPeriods);
            if (time.HasValue)
                ret = time.Value.TotalHours;
            return ret;
        }

        /// <summary>
        /// Gets the Scheduled time in the specified <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the scheduled time for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static TimeSpan? ScheduledTime(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            TimeSpan? ret = null;
            foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriods)
            {
            	TimeSpan toAdd =
                    TimeSpan.FromMinutes(skillStaffPeriod.CalculatedResource*
                                         skillStaffPeriod.Period.ElapsedTime().TotalMinutes);

            	ret = !ret.HasValue ? toAdd : ret.Value.Add(toAdd);
            }

            return ret;
        }

        /// <summary>
        /// Calculates the relative difference between the forecasted and scheduled hours in the specified <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the relative difference time for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static double? RelativeDifference(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            TimeSpan? forecastedTime = ForecastedTime(skillStaffPeriods);
            TimeSpan? scheduledTime = ScheduledTime(skillStaffPeriods);

            double forecastedMinutes = forecastedTime.HasValue ? forecastedTime.Value.TotalMinutes : 0;
            double scheduledMinutes = scheduledTime.HasValue ? scheduledTime.Value.TotalMinutes : 0;

            if (forecastedMinutes == 0 && scheduledMinutes == 0)
                return null;
            return new DeviationStatisticData(forecastedMinutes, scheduledMinutes).RelativeDeviation;
        }

        /// <summary>
        /// Calculates the relative difference between the forecasted and scheduled hours in the specified <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the relative difference time for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static double? RelativeDifferenceForDisplay(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            TimeSpan? forecastedTime = ForecastedTime(skillStaffPeriods);
            TimeSpan? scheduledTime = ScheduledTime(skillStaffPeriods);

            double forecastedMinutes = forecastedTime.HasValue ? forecastedTime.Value.TotalMinutes : 0;
            double scheduledMinutes = scheduledTime.HasValue ? scheduledTime.Value.TotalMinutes : 0;

            if (forecastedMinutes == 0 && scheduledMinutes == 0)
                return null;
            return new DeviationStatisticData(forecastedMinutes, scheduledMinutes).RelativeDeviationForDisplay;
        }

        /// <summary>
        /// Calculates the root mean square.
        /// </summary>
        /// <param name="intradayDifferences">The intraday differences (can be either absolut or relative).</param>
        /// <returns></returns>
        public static double? CalculateRootMeanSquare(IEnumerable<double> intradayDifferences)
        {
            double? result = null;

			if (intradayDifferences.Any())
            {
                result = Calculation.Variances.RMS(intradayDifferences);
            }
            return result;
        }

        public static double? SkillDayRootMeanSquare(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {

			IList<double> intradayDifferences = SkillStaffPeriodsAbsoluteDifferenceHours(skillStaffPeriods, false, false);
			return CalculateRootMeanSquare(intradayDifferences);
		}

        public static double? SkillDayGridSmoothness(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            if (!skillStaffPeriods.Any())
                return null;
            SkillStaffPeriodStatisticsForSkillIntraday statistics = new SkillStaffPeriodStatisticsForSkillIntraday(skillStaffPeriods);
            return statistics.StatisticsCalculator.RelativeStandardDeviation.Equals(double.NaN)
                       ? 0d
                       : statistics.StatisticsCalculator.RelativeStandardDeviation;
        }

		public static double? SkillPeriodGridSmoothness(IEnumerable<IEnumerable<ISkillStaffPeriod>> skillStaffPeriodsOfDays)
		{
			var relativeDifferences = new List<double>();
			foreach (var skillStaffPeriodsOfOneDay in skillStaffPeriodsOfDays)
			{
				var relativeDifference = RelativeDifference(skillStaffPeriodsOfOneDay);
				if (relativeDifference == null) continue;
				relativeDifferences.Add(relativeDifference.Value);
			}
			if (relativeDifferences.Count == 0)
				return null;

			return Calculation.Variances.StandardDeviation(relativeDifferences);
		}

        public static TimeSpan? AbsoluteDifferenceIncoming(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            TimeSpan? forecastedTime = ForecastedIncoming(skillStaffPeriods);
            TimeSpan? scheduledTime = ScheduledIncoming(skillStaffPeriods);

            if (forecastedTime.HasValue && scheduledTime.HasValue)
                return scheduledTime.Value.Subtract(forecastedTime.Value);

            return null;
        }

        public static double? RelativeDifferenceIncoming(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            TimeSpan? forecasted = ForecastedIncoming(skillStaffPeriods);
            TimeSpan? scheduled = ScheduledIncoming(skillStaffPeriods);

            if (forecasted.HasValue && scheduled.HasValue)
            {
                double f = forecasted.Value.TotalMinutes;
                double s = scheduled.Value.TotalMinutes;

                if (f == 0 && s == 0)
                    return null;
                return new DeviationStatisticData(f, s).RelativeDeviation;
            }

            return null;
        }

        public static TimeSpan? ForecastedIncoming(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            IList<double> list = skillStaffPeriods.Select(s => s.ForecastedIncomingDemand().TotalHours).ToList();
            return (list.Count > 0) ? TimeSpan.FromHours(list.Sum()) : (TimeSpan?)null;
        }

        public static TimeSpan? ScheduledIncoming(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            IList<double> list = skillStaffPeriods.Select(s => s.ScheduledAgentsIncoming * s.Period.ElapsedTime().TotalHours).ToList();
            return (list.Count > 0) ? TimeSpan.FromHours(list.Sum()) : (TimeSpan?)null;
        }

        public static Percent EstimatedServiceLevel(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            //Design: Magnus Karlsson

            double namer = 0;
            double talj = 0;

            foreach (ISkillStaffPeriod period in skillStaffPeriods)
            {
                namer = namer + period.EstimatedServiceLevel.Value * period.Payload.TaskData.Tasks;
                talj = talj + period.Payload.TaskData.Tasks;
            }

            if (talj > 0)
                return new Percent(namer / talj);
            return new Percent();
        }

		public static Percent EstimatedServiceLevelShrinkage(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
		{
			//Design: Magnus Karlsson

			double namer = 0;
			double talj = 0;

			foreach (ISkillStaffPeriod period in skillStaffPeriods)
			{
				namer = namer + period.EstimatedServiceLevelShrinkage.Value * period.Payload.TaskData.Tasks;
				talj = talj + period.Payload.TaskData.Tasks;
			}

			if (talj > 0)
				return new Percent(namer / talj);
			return new Percent();
		}
    }
}