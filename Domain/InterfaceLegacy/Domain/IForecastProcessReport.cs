using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// (Please fill in more appropiate desc here. I don't know myself...)
    /// Holds info of what periods the forecaster have worked with
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-09-30
    /// </remarks>
    public interface IForecastProcessReport
    {
        /// <summary>
        /// Gets the period collection.
        /// </summary>
        /// <value>The period collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-09-30
        /// </remarks>
        ICollection<DateOnlyPeriod> PeriodCollection { get; }
    }
}
