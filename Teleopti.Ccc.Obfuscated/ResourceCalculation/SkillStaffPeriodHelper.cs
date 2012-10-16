﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
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

        /// <summary>
        /// The calculation method how the Difference Hours are calculation from the ISkillStaffPeriods.
        /// </summary>
        private delegate IList<double> skillStaffPeriodsDifferenceHoursCalculationMethod(
            IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing);

        /// <summary>
        /// Gets the intraday absolut differences in hours between forecasted and scheduled resources in the specified
        /// <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <param name="considerMinStaffing">if set to <c>true</c> then consider the preset minimum staffing.</param>
        /// <param name="considerMaxStaffing">if set to <c>true</c> then consider the preset maximum staffing.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the absolut diference in hours for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static IList<double> SkillStaffPeriodsAbsoluteDifferenceHours(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing)
        {
            if(considerMinStaffing && considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.AbsoluteDifferenceBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            if(considerMinStaffing)
                return skillStaffPeriods.Select(s => s.AbsoluteDifferenceMinStaffBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            if (considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.AbsoluteDifferenceMaxStaffBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            return skillStaffPeriods.Select(s => s.AbsoluteDifference * s.Period.ElapsedTime().TotalHours).ToList();
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
        /// Gets the Forecasted hours in the specified <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the forecasted hours for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static double? ForecastedHours(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            var list = skillStaffPeriods.Select(s => s.FStaffHours()).ToList();
            return (list.Count > 0) ? list.Sum() : (double?)null;
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
        /// <param name="considerMinStaffing">if set to <c>true</c> consider min staffing.</param>
        /// <param name="considerMaxStaffing">if set to <c>true</c> consider max staffing.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the absolute difference time for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static TimeSpan? AbsoluteDifference(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing)
        {
            var list = SkillStaffPeriodsAbsoluteDifferenceHours(skillStaffPeriods, considerMinStaffing, considerMaxStaffing);
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
        /// Gets the specified person's active skills for the specified day.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="utcScheduleDate">The schedule date.</param>
        /// <returns></returns>
        public static IList<ISkill> PersonActiveSkillOnDay(IPerson person, DateTime utcScheduleDate)
        {
            // check for correct input data
            InParameter.NotNull("person", person);

            IList<ISkill> returnSkills = new List<ISkill>();
            TimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
            DateOnly scheduleDay = new DateOnly(TimeZoneHelper.ConvertFromUtc(utcScheduleDate, timeZoneInfo).Date);
            IPersonPeriod personPeriod = person.Period(scheduleDay);
            if (personPeriod == null) 
                return returnSkills;

            IList<IPersonSkill> personSkills = personPeriod.PersonSkillCollection;
            foreach (IPersonSkill personSkill in personSkills)
            {
                if (!returnSkills.Contains(personSkill.Skill))
                    returnSkills.Add(personSkill.Skill);
            }

            return returnSkills;
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
            return new DeviationStatisticData(forecastedMinutes, scheduledMinutes).RelativeDeviationForDisplayOnly;
        }

        /// <summary>
        /// Gets the highest intra interval deviation in the periods in the specified <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the the highest intra-interval deviation for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// </remarks>
        public static double? GetHighestIntraIntervalDeviation(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            return skillStaffPeriods.Any() ? skillStaffPeriods.Max(p => p.IntraIntervalDeviation) : (double?)null;
        }

        /// <summary>
        /// Calculates the root mean square from the absolute values.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <param name="considerMinStaffing">if set to <c>true</c> [consider min staffing].</param>
        /// <param name="considerMaxStaffing">if set to <c>true</c> [consider max staffing].</param>
        /// <param name="considerHighestIntraIntervalDeviation">if set to <c>true</c> [consider highest intra interval deviation].</param>
        /// <returns></returns>
        public static double? CalculateAbsoluteRootMeanSquare(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing, bool considerHighestIntraIntervalDeviation)
        {
            skillStaffPeriodsDifferenceHoursCalculationMethod calculationMethod = SkillStaffPeriodsAbsoluteDifferenceHours;
            return CalculateSkillStaffPeriodsRootMeanSquare(calculationMethod, skillStaffPeriods, considerMinStaffing,
                                                            considerMaxStaffing, considerHighestIntraIntervalDeviation);
        }

        /// <summary>
        /// Calculates the root mean square from the relatiove values.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <param name="considerMinStaffing">if set to <c>true</c> [consider min staffing].</param>
        /// <param name="considerMaxStaffing">if set to <c>true</c> [consider max staffing].</param>
        /// <param name="considerHighestIntraIntervalDeviation">if set to <c>true</c> [consider highest intra interval deviation].</param>
        /// <returns></returns>
        public static double? CalculateRelativeRootMeanSquare(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing, bool considerHighestIntraIntervalDeviation)
        {
            skillStaffPeriodsDifferenceHoursCalculationMethod calculationMethod = SkillStaffPeriodsRelativeDifferenceHours;
            return CalculateSkillStaffPeriodsRootMeanSquare(calculationMethod, skillStaffPeriods, considerMinStaffing,
                                                            considerMaxStaffing, considerHighestIntraIntervalDeviation);
        }

        /// <summary>
        /// Calculates the skill staff periods root mean square.
        /// </summary>
        /// <param name="calculationMethod">The calculation method.</param>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <param name="considerMinStaffing">if set to <c>true</c> [consider min staffing].</param>
        /// <param name="considerMaxStaffing">if set to <c>true</c> [consider max staffing].</param>
        /// <param name="considerHighestIntraIntervalDeviation">if set to <c>true</c> [consider highest intra interval deviation].</param>
        /// <returns></returns>
        private static double? CalculateSkillStaffPeriodsRootMeanSquare(skillStaffPeriodsDifferenceHoursCalculationMethod calculationMethod, IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing, bool considerHighestIntraIntervalDeviation)
        {
            double highestIntraIntervalDeviation = 0;
            IList<double> intradayDifferences = calculationMethod(skillStaffPeriods, considerMinStaffing, considerMaxStaffing);
            if (considerHighestIntraIntervalDeviation)
            {
                double? highestIntraIntervalDeviationOrNull = GetHighestIntraIntervalDeviation(skillStaffPeriods);
                highestIntraIntervalDeviation = highestIntraIntervalDeviationOrNull.HasValue ? highestIntraIntervalDeviationOrNull.Value : 0;
            }
            return CalculateRootMeanSquare(intradayDifferences, highestIntraIntervalDeviation);
        }

        /// <summary>
        /// Calculates the root mean square.
        /// </summary>
        /// <param name="intradayDifferences">The intraday differences (can be either absolut or relative).</param>
        /// <param name="highestIntraIntervalDeviation">The highest intra interval deviation.</param>
        /// <returns></returns>
        public static double? CalculateRootMeanSquare(IEnumerable<double> intradayDifferences, double highestIntraIntervalDeviation)
        {
            double? result = null;

            IPopulationStatisticsCalculator calculator = new PopulationStatisticsCalculator(intradayDifferences);

            if (calculator.Count > 0)
            {
                calculator.Analyze();
                result = calculator.RootMeanSquare;
                result += highestIntraIntervalDeviation;
            }
            return result;
        }

        #region SkillDayGrid

        public static double? SkillDayRootMeanSquare(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            return CalculateAbsoluteRootMeanSquare(skillStaffPeriods, false, false, false);
        }

        public static double? SkillDayGridSmoothness(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            if (skillStaffPeriods.Count() == 0)
                return null;
            SkillStaffPeriodStatisticsForSkillIntraday statistics = new SkillStaffPeriodStatisticsForSkillIntraday(skillStaffPeriods.ToList());
            statistics.Analyze();
            return statistics.StatisticsCalculator.RelativeStandardDeviation.Equals(double.NaN)
                       ? 0d
                       : statistics.StatisticsCalculator.RelativeStandardDeviation;
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

        #endregion

        /// note: currently not used
        /// <summary>
        /// Boosts AbsoluteDifference with 10000 times for each Min or Max staff that is broken.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-24
        /// </remarks>
        public static IList<double> IntradayMinMaxBoostedAbsoluteDifference(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            //Why should tis be in hours???
            IList<Double> ret = new List<double>();
            foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriods)
            {
                double traff = (skillStaffPeriod.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false) * 10000)
                                + skillStaffPeriod.AbsoluteDifference;
                double value = traff * skillStaffPeriod.Period.ElapsedTime().TotalHours;
                ret.Add(value);
            }

            return ret;
        }
    }
}