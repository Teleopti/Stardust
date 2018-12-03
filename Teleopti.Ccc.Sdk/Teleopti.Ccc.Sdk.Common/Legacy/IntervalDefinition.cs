using System;

// ReSharper disable once CheckNamespace
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Definition of an interval, with a local time as time span and the oringinal UTC time
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-12-16
    /// </remarks>
    public class IntervalDefinition
    {
        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>The date time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-12-16
        /// </remarks>
        public DateTime DateTime { get; private set; }

        /// <summary>
        /// Gets or sets the time span.
        /// </summary>
        /// <value>The time span.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-12-16
        /// </remarks>
        public TimeSpan TimeSpan { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalDefinition"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-12-16
        /// </remarks>
        public IntervalDefinition(DateTime dateTime, TimeSpan timeSpan)
        {
            DateTime = dateTime;
            TimeSpan = timeSpan;
        }
    }
}