using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// Merges the list of DateTimePeriods
    /// </summary>
    public class DateTimePeriodMerger
    {
        private readonly IList<DateTimePeriod> _periods;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriodMerger"/> class.
        /// </summary>
        /// <param name="periods">The periods.</param>
        public DateTimePeriodMerger(IList<DateTimePeriod> periods)
        {
            _periods = periods;
        }

        /// <summary>
        /// Merges the date time periods as days. It means that the offset will be set to a day, so if a period ends
        /// with one day and the next will start the next day then it will be considered as merge.
        /// </summary>
        /// <returns>the merged list</returns>
        public IList<DateTimePeriod> MergeDays()
        {
            return MergePeriods(new TimeSpan(1, 0, 0, 0, 0));
        }

        /// <summary>
        /// Merges the date time periods with the given offset where offset is the maximum time distance within two periods.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>the merged list</returns>
        public IList<DateTimePeriod> MergePeriods(TimeSpan offset)
        {
            /* 
             * - order periods
             * - save previous instance
             * - try to merge previous instance with the new and 
             * - add the merged or the unmerged instance to resultlist
            */


            DateTimePeriod? previousPeriod = null;
            IList<DateTimePeriod> resultList = new List<DateTimePeriod>();
            foreach (DateTimePeriod dateTimePeriod in OrderPeriods())
            {
                if (previousPeriod != null)
                {
                    DateTimePeriod? periodUnion = previousPeriod.Value.Union(dateTimePeriod, offset);
                    if (periodUnion.HasValue)
                    {
                        previousPeriod = periodUnion.Value;
                    }
                    else
                    {
                        resultList.Add(previousPeriod.Value);
                        previousPeriod = dateTimePeriod;
                    }
                }
                else
                {
                    previousPeriod = dateTimePeriod;
                }
            }
            if (previousPeriod != null)
            {
                resultList.Add(previousPeriod.Value);
            }

            return resultList;
        }

        /// <summary>
        /// Merges the date time periods.
        /// </summary>
        /// <returns>the merged list</returns>
        public IList<DateTimePeriod> OrderPeriods()
        {
            if (_periods.Count == 0)
                return _periods;
            var orderedPeriods = (
                from c in _periods 
                orderby c.StartDateTime ascending, c.EndDateTime ascending
                select c).ToList();
            return orderedPeriods;
        }
        
    }
}
