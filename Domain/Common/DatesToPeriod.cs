using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Converts dates to periods
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 2008-09-30
    /// </remarks>
    public class DatesToPeriod : IDatesToPeriod
    {
        /// <summary>
        /// Converts the specified date collection.
        /// </summary>
        /// <param name="dateCollection">The date collection.</param>
        /// <returns></returns>
        public ICollection<DateOnlyPeriod> Convert(IEnumerable<DateOnly> dateCollection)
        {
            ICollection<DateOnlyPeriod> periods = new List<DateOnlyPeriod>();
            DateOnly[] periodCollection = dateCollection.ToArray();

            if (periodCollection.Length > 0)
            {
                Array.Sort(periodCollection);
                DateOnly startTime = periodCollection[0];
                DateOnly previousDate = periodCollection[0];

                for (int t = 1; t < periodCollection.Length; t++)
                {
                    DateOnly date = periodCollection[t];

                    if ((previousDate.AddDays(1)) != date)
                    {
                        var nextPeriod = new DateOnlyPeriod(startTime, previousDate);
                        periods.Add(nextPeriod);

                        startTime = date;
                        previousDate = date;
                    }
                    else
                    {
                        previousDate = date;
                    }
                }
                
                var period = new DateOnlyPeriod(startTime, previousDate);

                periods.Add(period);
            }

            return periods;
        }
    }
}
