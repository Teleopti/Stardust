using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Class for holding a payload and a timespan
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: micke
    /// Created date: 16.12.2007
    /// </remarks>
    public class PayloadTime<T>
    {
        private T _payload;
        private TimeSpan _time;

        private PayloadTime() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadTime&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="time">The time.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 16.12.2007
        /// </remarks>
        public PayloadTime(T payload, TimeSpan time)
        {
            _time = time;
            _payload = payload;
        }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>The payload.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 16.12.2007
        /// </remarks>
        public T Payload
        {
            get { return _payload; }
        }

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <value>The time.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 16.12.2007
        /// </remarks>
        public TimeSpan Time
        {
            get { return _time; }
        }
    }
}
