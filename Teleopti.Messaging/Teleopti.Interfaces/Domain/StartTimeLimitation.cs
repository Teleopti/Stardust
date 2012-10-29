﻿using System;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Used for limitations of start times in for example Rotations
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-10-15    
    /// /// </remarks>
    [Serializable]
    public struct StartTimeLimitation : ILimitation
    {
    	private readonly TimeSpan? _startTime;
        private readonly TimeSpan? _endTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartTimeLimitation"/> struct.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// </remarks>
		public StartTimeLimitation(TimeSpan? startTime, TimeSpan? endTime)
        {
			_startTime = startTime;
            _endTime = endTime;
				verifyTimes(startTime, endTime);
		  }

		private static readonly TimeSpan verifyLimit = new TimeSpan(23, 59, 59);
		private static void verifyTimes(TimeSpan? startTime, TimeSpan? endTime)
		{
			if (startTime.HasValue)
			{
				if (startTime.Value > verifyLimit)
					throw new ArgumentOutOfRangeException("startTime", startTime, "Start Time can't be bigger than 23:59:59");

				if (endTime.HasValue && startTime > endTime.Value)
					throw new ArgumentOutOfRangeException("startTime", startTime, "Start Time can't be greater than End Time");
			}

			if (endTime.HasValue)
			{
				if (endTime.Value > verifyLimit)
					throw new ArgumentOutOfRangeException("endTime", endTime, "End Time can't be bigger than 23:59:59");
			}
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
        }


        /// <summary>
        /// Sets the start time string.
        /// </summary>
        /// <value>The start time string.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
        public string StartTimeString
        {
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
        public static bool operator ==(StartTimeLimitation per1, StartTimeLimitation per2)
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
        public static bool operator !=(StartTimeLimitation per1, StartTimeLimitation per2)
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
                result = (result * 351) ^ EndTime.GetHashCode();
                result = (result * 351) ^ StartTime.GetHashCode();
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
        public bool Equals(StartTimeLimitation other)
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
            if (obj == null || !(obj is StartTimeLimitation))
            {
                return false;
            }
            return Equals((StartTimeLimitation)obj);
        }

        /// <summary>
        /// </summary>
        /// <param name="value">The time string.</param>
        /// <returns></returns>
        /// <remarks>
        /// Throws ArgumentOutOfRangeException if unable to parse
        /// Created by: henrika
        /// Created date: 2009-01-26
        /// </remarks>
		public TimeSpan? TimeSpanFromString(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            TimeSpan timeSpan;
            if (TimeHelper.TryParse(value, out timeSpan))
            {
				verifyTimes(timeSpan, null);
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
				ret = TimeHelper.TimeOfDayFromTimeSpan((TimeSpan)value, CultureInfo.CurrentCulture);
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



		  private static readonly TimeSpan oneDay = TimeSpan.FromDays(1);

    	/// <summary>
    	/// Determines if limitation is valid for <paramref name="timeSpan"/>
    	/// </summary>
    	public bool IsValidFor(TimeSpan timeSpan)
		 {
		 	var startTimeHasValue = StartTime.HasValue;
		 	var endTimeHasValue = EndTime.HasValue;
			if (startTimeHasValue && endTimeHasValue)
			{
				return new TimePeriod(StartTime.Value, EndTime.Value).ContainsPart(timeSpan);
			}
			if (startTimeHasValue)
			{
				return new TimePeriod(StartTime.Value, oneDay).ContainsPart(timeSpan); 
			}
			if (endTimeHasValue)
			{
				return timeSpan <= EndTime.Value;				
			}
			return timeSpan < oneDay;
		 }

		/// <summary>
		/// To's the string
		/// </summary>
		public override string ToString() { return StartTimeString + "-" + EndTimeString; }

    }
}
