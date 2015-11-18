using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Distributes the task times.
        /// </summary>
        /// <param name="changeAsPercent">The change as percent.</param>
        /// <param name="targets">The targets.</param>
        /// <param name="taskFieldToDistribute">The task field to distribute.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        public static void DistributeTaskTimes(double changeAsPercent, IEnumerable targets, TaskFieldToDistribute taskFieldToDistribute, Double originalValue)
        {
            DistributeTaskTimes(changeAsPercent, TimeSpan.Zero, targets, taskFieldToDistribute, DistributionType.ByPercent, originalValue);
        }

        /// <summary>
        /// Distributes the task times.
        /// </summary>
        /// <param name="changeAsPercent">The change as percent.</param>
        /// <param name="newTime">The new time.</param>
        /// <param name="targets">The targets.</param>
        /// <param name="taskFieldToDistribute">The task field to distribute.</param>
        /// <param name="distributionType">Type of the distribution.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        public static void DistributeTaskTimes(double changeAsPercent, TimeSpan newTime, IEnumerable targets, TaskFieldToDistribute taskFieldToDistribute, DistributionType distributionType, double originalValue)
        {
            if (newTime < TimeSpan.Zero) throw new ArgumentOutOfRangeException("newTime", "The new time cannot be negative. Use TimeSpan.Zero instead.");
			IList<IForecastingTarget> typedTargets = targets.OfType<IForecastingTarget>().ToList();

            int numberOfTargets = typedTargets.Count();
            if (numberOfTargets == 0) return;

			PropertyInfo property = typeof(IForecastingTarget).GetProperty(taskFieldToDistribute.ToString());
			foreach (IForecastingTarget owner in typedTargets)
            {
                long averageTaskTimeTicks = ((TimeSpan)property.GetValue(owner, null)).Ticks;
                if (distributionType == DistributionType.ByPercent)
                {
                    if (averageTaskTimeTicks == 0)
                        averageTaskTimeTicks = (long)originalValue;  //TimeSpan.FromSeconds(1).Ticks;
                    else
                    averageTaskTimeTicks = (long)(averageTaskTimeTicks * changeAsPercent);
                }
                else
                {
                    averageTaskTimeTicks = newTime.Ticks;
                }
                property.SetValue(owner, TimeSpan.FromTicks(averageTaskTimeTicks), null);
            }
        }

	    public static void DistributeToFirstOpenPeriod(double newTotal, IEnumerable<ITemplateTaskPeriod> targets, IList<TimePeriod> openHourList)
	    {
		    var targetList = targets.ToList();
			targetList.ForEach(t => t.Tasks = 0);
			var firstSuitableTarget = targetList.FirstOrDefault(x => openHourList.Any(y => x.Period.TimePeriod(TimeZoneInfo.Utc).Intersect(y)));
		    if (firstSuitableTarget != null)
		    {
			    firstSuitableTarget.Tasks = newTotal;
		    }		    
	    }

		public static void DistributeOverrideTasks(double? newTotal, IEnumerable<ITaskOwner> targets, IEnumerable<IForecastingTarget> intradayCallPattern)
		{
			var typedTargets = targets.OfType<ITaskOwner>();
			var dailyTasks = typedTargets.Sum(t => t.Tasks);
			if (Math.Floor(dailyTasks) > 0)
			{
				typedTargets.ForEach(t => t.SetOverrideTasks((t.Tasks / dailyTasks) * newTotal, null));
			}
			else
			{
				if (intradayCallPattern != null)
				{					

					var intradayCallPatternArray = intradayCallPattern.ToArray();
					int i = 0;
					var patternTasks = intradayCallPatternArray.Sum(t => t.Tasks);
					typedTargets.ForEach(t =>
					{
						t.SetOverrideTasks((intradayCallPatternArray[i].Tasks / patternTasks) * newTotal, null);
						i++;
					});

				}
				else
				{
					typedTargets.ForEach(t => t.SetOverrideTasks((newTotal / targets.ToList().Count), null));
				}
			}
		}
    }

    /// <summary>
    /// Determines which field to work with when distributing task times
    /// </summary>
    public enum TaskFieldToDistribute
    {
        /// <summary>
        /// Average task time
        /// </summary>
        AverageTaskTime,
        /// <summary>
        /// Average after task time
        /// </summary>
        AverageAfterTaskTime
    }
}
