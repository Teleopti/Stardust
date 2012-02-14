using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// A payload within a TimePeriod
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TimeLayer<T> : ITimeLayer<T>
    {
        private readonly TimePeriod _period;
        private readonly T _payload;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeLayer&lt;T&gt;"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        protected internal TimeLayer(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeLayer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        protected TimeLayer(T payload, TimePeriod period)
        {
            InParameter.NotNull("payload", payload);
            _period = period;
            _payload = payload;
        }

        /// <summary>
        /// Gets the pay load.
        /// </summary>
        /// <value>The pay load.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual T Payload
        {
            get { return _payload; }
        }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual TimePeriod Period
        {
            get { return _period; }
        }
    }
}
