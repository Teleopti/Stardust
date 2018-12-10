using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Processes ´forecast reports
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 2008-10-01
    /// </remarks>
    public class ForecastProcessReport : IForecastProcessReport
    {
        private readonly ICollection<DateOnlyPeriod> periodCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForecastProcessReport"/> class.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 2008-10-01
        /// </remarks>
        public ForecastProcessReport(ICollection<DateOnlyPeriod> periods)
        {
            periodCollection = periods;
        }

        /// <summary>
        /// Gets the period collection.
        /// </summary>
        /// <value>The period collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-09-30
        /// </remarks>
        public ICollection<DateOnlyPeriod> PeriodCollection
        {
            get { return periodCollection; }
        }
    }
}
