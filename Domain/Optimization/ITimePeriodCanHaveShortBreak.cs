using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Contatins a merthod that decides whether the TimePeriodWithSegment has rast
    /// </summary>
    public interface ITimePeriodCanHaveShortBreak
    {
        /// <summary>
        /// Determines whether the specified skill extractor  and timeperiod with segment has rast.
        /// </summary>
        /// <param name="skillExtractor">The skill extractor.</param>
        /// <param name="timePeriodWithSegment">The time period with segment.</param>
        /// <returns>
        /// 	<c>true</c> if the specified skill extractor has rast; otherwise, <c>false</c>.
        /// </returns>
        bool CanHaveShortBreak(ISkillExtractor skillExtractor, TimePeriodWithSegment timePeriodWithSegment);

        bool CanHaveShortBreak(ISkillExtractor skillExtractor, IEnumerable<TimeSpan> timeSpans);
    }
}