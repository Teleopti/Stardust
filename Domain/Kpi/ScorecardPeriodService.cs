using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Kpi
{

    /// <summary>
    /// A helper just to crate and return a list of different periods
    /// a scorecard can have
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-04-17    
    /// /// </remarks>
    public static class ScorecardPeriodService
    {
        /// <summary>
        /// Scorecards the period list.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        public static IEnumerable<IScorecardPeriod> ScorecardPeriodList()
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new ScorecardPeriod(i);
            }
        }
    }
}
