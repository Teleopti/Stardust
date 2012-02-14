using System;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Event arguments containing a defined type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-07-21
    /// </remarks>
    public class CustomEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEventArgs&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        public CustomEventArgs(T value)
        {
            _value = value;
        }

        private readonly T _value;

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        public T Value
        {
            get { return _value; }
        }
    }
}