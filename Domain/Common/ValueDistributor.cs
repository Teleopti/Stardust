using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

	    public static void DistributeToFirstOpenPeriod(double newTotal, IEnumerable<ITemplateTaskPeriod> targets, ICollection<TimePeriod> openHourList, TimeZoneInfo targetTimeZoneInfo)
	    {
		    var targetList = targets.ToList();
			targetList.ForEach(t => t.Tasks = 0);
			var firstSuitableTarget = targetList.FirstOrDefault(x => openHourList.Any(y => x.Period.TimePeriod(targetTimeZoneInfo).Intersect(y)));
		    if (firstSuitableTarget != null)
		    {
			    firstSuitableTarget.Tasks = newTotal;
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
}
