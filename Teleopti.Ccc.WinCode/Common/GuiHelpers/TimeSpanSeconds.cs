using System;
using System.Globalization;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    /// <summary>
    /// Clas to handle TimeSpan and TimeSpan Seconds
    /// </summary>
    public class TimeSpanSeconds
    {
        private TimeSpan _timeSpan;


        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanSeconds"/> class.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-12-20
        /// </remarks>
        public TimeSpanSeconds(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanSeconds"/> struct.
        /// </summary>
        /// <param name="seconds">The seconds.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-12-21
        /// </remarks>
        public TimeSpanSeconds(string seconds)
        {
            _timeSpan = TimeSpan.FromSeconds(Convert.ToDouble(seconds,CultureInfo.CurrentCulture));
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="System.TimeSpan"/> to <see cref="TimeSpanSeconds"/>.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-12-21
        /// </remarks>
        static public implicit operator TimeSpanSeconds(TimeSpan timeSpan)
        {
            return new TimeSpanSeconds(timeSpan);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="TimeSpanSeconds"/>.
        /// </summary>
        /// <param name="seconds">The seconds respresented in a string.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-12-21
        /// </remarks>
        static public implicit operator TimeSpanSeconds(string seconds)
        {
            return new TimeSpanSeconds(seconds);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="TimeSpanSeconds"/> to <see cref="System.Double"/>.
        /// </summary>
        /// <param name="timeSpanSeconds">The seconds time span item.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-12-21
        /// </remarks>
        static public implicit operator double(TimeSpanSeconds timeSpanSeconds)
        {
            return timeSpanSeconds.Seconds;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="TimeSpanSeconds"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="timeSpanSeconds">The seconds time span item.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-12-21
        /// </remarks>
        static public implicit operator string(TimeSpanSeconds timeSpanSeconds)
        {
            return timeSpanSeconds.Seconds.ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="TimeSpanSeconds"/> to <see cref="System.TimeSpan"/>.
        /// </summary>
        /// <param name="timeSpanSeconds">The seconds time span item.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-12-21
        /// </remarks>
        static public implicit operator TimeSpan(TimeSpanSeconds timeSpanSeconds)
        {
            return timeSpanSeconds._timeSpan;
        }

        /// <summary>
        /// Gets or sets the time span.
        /// </summary>
        /// <value>The time span.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-12-20
        /// </remarks>
        public TimeSpan TimeSpan
        {
            get { return _timeSpan; }
            set { _timeSpan = value; }
        }

        /// <summary>
        /// Gets or sets the seconds.
        /// </summary>
        /// <value>The seconds.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-12-20
        /// </remarks>
        public double Seconds
        {
            get
            {
                return _timeSpan.TotalMilliseconds / 1000;
            }
            set
            {
                _timeSpan = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-12-20
        /// </remarks>
        public override string ToString()
        {
            return Seconds.ToString(CultureInfo.CurrentCulture);
        }

    }
}
