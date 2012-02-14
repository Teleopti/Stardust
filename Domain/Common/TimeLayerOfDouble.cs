using System;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// A TimeLayer containing a Double
    /// </summary>
    public class TimeLayerOfDouble: TimeLayer<double>, ITimeLayerOfDouble
    {
        private TimeLayerOfDouble(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeLayerOfDouble"/> class.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        public TimeLayerOfDouble(Double payload, TimePeriod period) : base(payload, period)
        {

        }

    }
}
