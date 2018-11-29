using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
        private readonly TimeSpan? _startTime;
		  private readonly TimeSpan? _endTime;


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
            _startTime = startTime;
			_endTime = endTime;
			verifyTimes(startTime, endTime);
		  }

	    public static readonly TimeSpan VerifyLimit = new TimeSpan(36, 0, 0);
		private static void verifyTimes(TimeSpan? startTime, TimeSpan? endTime)
		{
			if (startTime.HasValue)
			{
				if (startTime.Value > VerifyLimit)
					throw new ArgumentOutOfRangeException(nameof(startTime), startTime, "Min Time can't be bigger than 36 hours");

				if (endTime.HasValue && startTime > endTime.Value)
					throw new ArgumentOutOfRangeException(nameof(startTime), startTime, "Start time can't be greater than end time");
			}

			if (!endTime.HasValue) return;
			if (endTime.Value > VerifyLimit)
				throw new ArgumentOutOfRangeException(nameof(endTime), endTime, "End time can't be bigger than 36 hours");
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
		public TimeSpan? StartTime => _startTime;

	    /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
		public TimeSpan? EndTime => _endTime;

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
	        if (_startTime?.CompareTo(timeSpanToCompareTo) > 0)
		        return false;
	        if (_endTime?.CompareTo(timeSpanToCompareTo) < 0)
		        return false;
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
        public string StartTimeString => StringFromTimeSpan(StartTime);
		
	    /// <summary>
        /// Gets or sets the end time string.
        /// </summary>
        /// <value>The end time string.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
        public string EndTimeString => StringFromTimeSpan(EndTime);

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
	        return other._startTime == _startTime && other._endTime == _endTime;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
		{
			return obj is WorkTimeLimitation limitation && Equals(limitation);
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
            return StartTime.HasValue || EndTime.HasValue;
        }

 

		 private static readonly TimeSpan OneAndHalfDay = TimeSpan.FromHours(36);

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
				  return new TimePeriod(StartTime.Value, OneAndHalfDay).ContainsPart(timeSpan);
			  }
			  if (endTimeHasValue)
			  {
				  return timeSpan <= EndTime.Value;
			  }
			  return timeSpan < OneAndHalfDay;
		  }

		/// <summary>
		/// To's the string
		/// </summary>
		public override string ToString() { return StartTimeString + "-" + EndTimeString; }

    }
}

