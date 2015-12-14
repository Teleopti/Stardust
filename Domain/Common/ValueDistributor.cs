using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Distribute from a total to all child objects
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-13
    /// </remarks>
    public static class ValueDistributor
    {
        /// <summary>
        /// Distributes the specified new total.
        /// </summary>
        /// <param name="newTotal">The new total.</param>
        /// <param name="targets">The targets.</param>
        /// <param name="distributionType">Type of distribution.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        public static void Distribute(double newTotal, IEnumerable targets, DistributionType distributionType)
        {
            IEnumerable<ITaskOwner> typedTargets = targets.OfType<ITaskOwner>();
            int numberOfTargets = typedTargets.Count();
            if (numberOfTargets == 0) return;

            double sumOfCurrentValues = typedTargets.Sum(t => t.Tasks);
            if (sumOfCurrentValues == 0d &&
                distributionType == DistributionType.ByPercent)
            {
                //Distribute values even if there are no available task numbers
                Distribute(newTotal, targets, DistributionType.Even);
                return;
            }

            switch (distributionType)
            {
                case DistributionType.Even:
                    double add = (newTotal - sumOfCurrentValues) / numberOfTargets;
                    typedTargets.ForEach(t => t.Tasks = t.Tasks + add);
                    break;
                case DistributionType.ByPercent:
                    typedTargets.ForEach(t => t.Tasks = (t.Tasks / sumOfCurrentValues) * newTotal);
                    break;
            }
        }

        
		public static void DistributeAverageTaskTime(double changeAsPercent, TimeSpan newTime, IEnumerable targets, DistributionType distributionType)
		{
			averageTaskTimeCheck(newTime);

			int numberOfTargets;
			var typedTargets = forecastingTargets(targets, out numberOfTargets);
			if (numberOfTargets == 0) return;

			foreach (var owner in typedTargets)
			{
				var averageTaskTimeTicks = distributeTimesForInterval(changeAsPercent, newTime, distributionType, owner.AverageTaskTime.Ticks);
				owner.AverageTaskTime = TimeSpan.FromTicks(averageTaskTimeTicks);
			}
		}

		public static void DistributeAverageAfterTaskTime(double changeAsPercent, TimeSpan newTime, IEnumerable targets, DistributionType distributionType)
		{
			averageTaskTimeCheck(newTime);

			int numberOfTargets;
			var typedTargets = forecastingTargets(targets, out numberOfTargets);
			if (numberOfTargets == 0) return;

			foreach (var owner in typedTargets)
			{
				var averageAfterTaskTimeTicks = distributeTimesForInterval(changeAsPercent, newTime, distributionType, owner.AverageAfterTaskTime.Ticks);
				owner.AverageAfterTaskTime = TimeSpan.FromTicks(averageAfterTaskTimeTicks);
			}
		}

	    public static void DistributeToFirstOpenPeriod(double newTotal, IEnumerable<ITemplateTaskPeriod> targets, IList<TimePeriod> openHourList, TimeZoneInfo targetTimeZoneInfo)
	    {
		    var targetList = targets.ToList();
			targetList.ForEach(t => t.Tasks = 0);
			var firstSuitableTarget = targetList.FirstOrDefault(x => openHourList.Any(y => x.Period.TimePeriod(targetTimeZoneInfo).Intersect(y)));
		    if (firstSuitableTarget != null)
		    {
			    firstSuitableTarget.Tasks = newTotal;
		    }		    
	    }

	    public static void DistributeOverrideTasks(double? newTotal, IEnumerable<ITaskOwner> targets, IEnumerable<IForecastingTarget> intradayCallPattern)
	    {
		    var typedTargets = targets.ToList();
		    var intradayPattern = intradayCallPattern == null ? null : intradayCallPattern.ToList(); 
			var dailyTasks = typedTargets.Sum(t => t.Tasks);
			if (Math.Floor(dailyTasks) > 0)
			{
				typedTargets.ForEach(t => t.SetOverrideTasks((t.Tasks / dailyTasks) * newTotal, null));
			}
			else
			{
				if (intradayPattern != null)
				{
					IList<IForecastingTarget> intradayCallPatternArray = new List<IForecastingTarget>();
					if (typedTargets.Count != intradayPattern.Count)
					{
						foreach (var typedTarget in typedTargets)
						{
							var patternSelection = intradayPattern.First(
								t => ((IPeriodized) t).Period.StartDateTime.TimeOfDay == ((IPeriodized) typedTarget).Period.StartDateTime.TimeOfDay);
							intradayCallPatternArray.Add(patternSelection);
						}
					}
					else
					{
						intradayCallPatternArray = intradayPattern;
					}
					var i = 0;
					var patternTasks = intradayCallPatternArray.Sum(t => t.Tasks);
					if (Math.Floor(patternTasks) > 0)
					{
						typedTargets.ForEach(t =>
						{
							t.SetOverrideTasks((intradayCallPatternArray[i].Tasks/patternTasks)*newTotal, null);
							i++;
						});
					}
					else
					{
						typedTargets.ForEach(t => t.SetOverrideTasks((newTotal / typedTargets.Count), null));
					}
				}
				else
				{
					typedTargets.ForEach(t => t.SetOverrideTasks((newTotal / typedTargets.Count), null));
				}
			}
		}

		public static void DistributeOverrideAverageTaskTime(double changeAsPercent, TimeSpan? newTime, IEnumerable targets, DistributionType distributionType)
		{
			int numberOfTargets;
			var typedTargets = forecastingTargets(targets, out numberOfTargets);
			if (numberOfTargets == 0) return;

			if (newTime.HasValue)
			{
				averageTaskTimeCheck(newTime.Value);
				foreach (var owner in typedTargets)
				{
					var originalValueTicks = owner.OverrideAverageTaskTime.HasValue ? owner.OverrideAverageTaskTime.Value.Ticks : 0;
					var overrideAverageTaskTimeTicks = distributeTimesForInterval(changeAsPercent, newTime.Value, distributionType, originalValueTicks);
					owner.OverrideAverageTaskTime = TimeSpan.FromTicks(overrideAverageTaskTimeTicks);
				}
			}
			else
			{
				foreach (var owner in typedTargets)
				{
					owner.OverrideAverageTaskTime = null;
				}
			}
	    }

		public static void DistributeOverrideAverageAfterTaskTime(double changeAsPercent, TimeSpan? newTime, IEnumerable targets, DistributionType distributionType)
	    {
			int numberOfTargets;
			var typedTargets = forecastingTargets(targets, out numberOfTargets);
			if (numberOfTargets == 0) return;

			if (newTime.HasValue)
			{
				averageTaskTimeCheck(newTime.Value);
				foreach (var owner in typedTargets)
				{
					var originalValueTicks = owner.OverrideAverageAfterTaskTime.HasValue ? owner.OverrideAverageAfterTaskTime.Value.Ticks : 0;
					var overrideAverageAfterTaskTimeTicks = distributeTimesForInterval(changeAsPercent, newTime.Value, distributionType, originalValueTicks);
					owner.OverrideAverageAfterTaskTime = TimeSpan.FromTicks(overrideAverageAfterTaskTimeTicks);
				}
			}
			else
			{
				foreach (var owner in typedTargets)
				{
					owner.OverrideAverageAfterTaskTime = null;
				}
			}
	    }

		private static void averageTaskTimeCheck(TimeSpan newTime)
		{
			if (newTime < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException("newTime", "The new time cannot be negative. Use TimeSpan.Zero instead.");
		}

		private static IEnumerable<IForecastingTarget> forecastingTargets(IEnumerable targets, out int numberOfTargets)
		{
			IList<IForecastingTarget> typedTargets = targets.OfType<IForecastingTarget>().ToList();

			numberOfTargets = typedTargets.Count();
			return typedTargets;
		}

		private static long distributeTimesForInterval(double changeAsPercent, TimeSpan newTime,
			DistributionType distributionType, long originalAverageTaskTimeTicks)
		{
			long newAverageTaskTimeTicks;
			if (distributionType == DistributionType.ByPercent)
			{
				if (originalAverageTaskTimeTicks == 0)
					newAverageTaskTimeTicks = newTime.Ticks;
				else
					newAverageTaskTimeTicks = (long)(originalAverageTaskTimeTicks * changeAsPercent);
			}
			else
			{
				newAverageTaskTimeTicks = newTime.Ticks;
			}
			return newAverageTaskTimeTicks;
		}

    }

    /// <summary>
    /// Determines which field to work with when distributing task times
    /// </summary>
    public enum TaskFieldToDistribute
    {
        AverageTaskTime,
        AverageAfterTaskTime,
		OverrideAverageTaskTime,
		OverrideAverageAfterTaskTime
    }
}
