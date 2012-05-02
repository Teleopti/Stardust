﻿using System;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2009-04-14
    /// </remarks>
    [Serializable]
    public struct WorkTimeLimitation : ILimitation
    {
        private PositiveTimeSpan? _startTime;
		private PositiveTimeSpan? _endTime;


        /// <summary>
        /// Initializes a new instance of the <see cref="WorkTimeLimitation"/> struct.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-14
        /// </remarks>
		public WorkTimeLimitation(TimeSpan? startTime, TimeSpan? endTime)
        {
            _startTime = (PositiveTimeSpan?) startTime;
			_endTime = (PositiveTimeSpan?) endTime;
        }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
		public TimeSpan? StartTime
        {
			get { return _startTime; }
            set
            {
                if (value.HasValue)
                {
                    if (value > new TimeSpan(23, 59, 59))
                        throw new ArgumentOutOfRangeException("value", value, "Start Time can't be bigger than 23.59.59");

                    if (_endTime.HasValue && value > _endTime.Value)
						throw new ArgumentOutOfRangeException("value", value, "Start Time can't be greater than End Time");
                }
				_startTime = (PositiveTimeSpan?) value;
            }
        }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
		public TimeSpan? EndTime
        {
            get { return _endTime; }
            set
            {
                if (value.HasValue)
                {
                    if (value > new TimeSpan(23, 59, 59))
                        throw new ArgumentOutOfRangeException("value", "End Time can't be bigger than 23.59.59");

					if (_startTime.HasValue && value < _startTime.Value)
						throw new ArgumentOutOfRangeException("value", "End Time can't less than Start Time");
                }
				_endTime = (PositiveTimeSpan?) value;
            }
        }

        /// <summary>
        /// Determines whether the supplied value matches the limitation.
        /// </summary>
        /// <param name="timeSpanToCompareTo">The time span to compare to.</param>
        /// <returns>
        /// 	<c>true</c> if [is corresponding to work time limitation] [the specified time span to compare to]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-15
        /// </remarks>
		public bool IsCorrespondingToWorkTimeLimitation(TimeSpan timeSpanToCompareTo)
        {
            if (_startTime.HasValue)
            {
                if (((TimeSpan)_startTime.Value).CompareTo(timeSpanToCompareTo) > 0)
                    return false;
            }
            if (_endTime.HasValue)
            {
                if (((TimeSpan)_endTime.Value).CompareTo(timeSpanToCompareTo) < 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets or sets the start time string.
        /// </summary>
        /// <value>The start time string.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
        public string StartTimeString
        {
            set
            {
                StartTime = TimeSpanFromString(value);
            }
            get
            {
                return StringFromTimeSpan(StartTime);
            }
        }


        /// <summary>
        /// Gets or sets the end time string.
        /// </summary>
        /// <value>The end time string.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
        public string EndTimeString
        {
            set
            {
                EndTime = TimeSpanFromString(value);
            }

            get
            {
                return StringFromTimeSpan(EndTime);
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-16    
        /// /// </remarks>
        public static bool operator ==(WorkTimeLimitation per1, WorkTimeLimitation per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-16    
        /// /// </remarks>
        public static bool operator !=(WorkTimeLimitation per1, WorkTimeLimitation per2)
        {
            return !per1.Equals(per2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                result = (result * 350) ^ EndTime.GetHashCode();
                result = (result * 350) ^ StartTime.GetHashCode();
                return result;
            }
        }


        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-16    
        /// /// </remarks>
        public bool Equals(WorkTimeLimitation other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-16    
        /// /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is WorkTimeLimitation))
            {
                return false;
            }
            return Equals((WorkTimeLimitation)obj);
        }

        /// <summary>
        /// </summary>
        /// <param name="value">The time string.</param>
        /// <returns></returns>
        /// <remarks>
        /// Throws ArgumentOutOfRangeException if not able to parse
        /// Created by: henrika
        /// Created date: 2009-01-26
        /// </remarks>
        public TimeSpan? TimeSpanFromString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (value.StartsWith("24", StringComparison.OrdinalIgnoreCase))
                value = "00:00 +1";
            TimeSpan timeSpan;
            if (TimeHelper.TryParse(value, out timeSpan))
            {
                return timeSpan;
            }

            throw new ArgumentOutOfRangeException("value", value, "The string can not be converted to a TimeSpan");

        }

        /// <summary>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Exposed for handling Validation in the presentation-layer
        /// Created by: henrika
        /// Created date: 2009-01-27
        /// </remarks>
        public string StringFromTimeSpan(TimeSpan? value)
        {
            string ret = "";
            if (value.HasValue)
            {
				ret = TimeHelper.GetLongHourMinuteTimeString(value.Value, CultureInfo.CurrentCulture);
            }
            return ret;
        }

        /// <summary>
        /// Determines whether this instance has value.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if StartTime or EndTime has value; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-28
        /// </remarks>
        public bool HasValue()
        {
            return StartTime.HasValue | EndTime.HasValue;
        }

        /// <summary>
        /// Returns a valid timeperiod limited by the minimum and/or maximum values defined by the limiter.
        /// </summary>
        /// <returns></returns>
        public TimePeriod ValidPeriod()
        {
			TimeSpan startTime = TimeSpan.Zero;
            TimeSpan endTime = TimeSpan.FromHours(36);

            if (StartTime.HasValue)
                startTime = StartTime.Value;

            if (EndTime.HasValue)
                endTime = EndTime.Value;

            return new TimePeriod(startTime, endTime);
        }

    }
}

