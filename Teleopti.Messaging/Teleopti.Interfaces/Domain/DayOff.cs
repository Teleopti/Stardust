using System;
using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
    public class DayOff : IEquatable<DayOff>
    {
        #region Fields

        private DateTime _anchor;
        private TimeSpan _targetLength;
        private TimeSpan _flexibility;
        private Description _description;
        private Color _displayColor;
		private readonly string _payrollCode;

    	#endregion

        #region Properties

        /// <summary>
        /// Gets the flexibility.
        /// </summary>
        public TimeSpan Flexibility
        {
            get { return _flexibility; }
        }

        /// <summary>
        /// Gets the length (duration).
        /// </summary>
        public TimeSpan TargetLength
        {
            get { return _targetLength; }
        }

        /// <summary>
        /// Gets the anchor point as date and time.
        /// </summary>
        public DateTime Anchor
        {
            get { return _anchor; }
        }

        /// <summary>
        /// Description
        /// </summary>
        public Description Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Gets the anchor with current time zone applied.
        /// </summary>
        /// <value>The anchor local.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-22
        /// </remarks>
        public DateTime AnchorLocal(TimeZoneInfo targetTimeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(_anchor, targetTimeZone);
         }

        /// <summary>
        /// Gets the displayColor
        /// </summary>
        public Color DisplayColor
        {
            get { return _displayColor; }
        }

        #endregion

    	/// <summary>
    	/// Initializes a new instance
    	/// </summary>
    	/// <param name="anchor">The anchor.</param>
    	/// <param name="targetLength">Length of the target.</param>
    	/// <param name="flexibility">The flexibility.</param>
    	/// <param name="description">The description.</param>
    	/// <param name="displayColor">The display color.</param>
    	/// <param name="payrollCode">The payroll code.</param>
    	public DayOff(DateTime anchor, TimeSpan targetLength, TimeSpan flexibility, Description description, Color displayColor,  string payrollCode)
        {
            InParameter.VerifyDateIsUtc("anchor", anchor);

            _anchor = anchor;
            _targetLength = targetLength;
            if (flexibility.TotalMinutes > targetLength.TotalMinutes / 2d)
                flexibility = TimeSpan.FromMinutes(targetLength.TotalMinutes / 2);
            _flexibility = flexibility;
            _description = description;
            _displayColor = displayColor;
			_payrollCode = payrollCode;
        }

        /// <summary>
        /// Gets the boundary.
        /// </summary>
        /// <value>The boundary.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        public DateTimePeriod Boundary
        {
            get
            {
                double minutes = (_targetLength.TotalMinutes / 2d) + (_flexibility.TotalMinutes);

                return new DateTimePeriod(
                    _anchor.AddMinutes(-minutes),
                    _anchor.AddMinutes(minutes));
            }
        }

        /// <summary>
        /// Gets the inner boundary.
        /// </summary>
        /// <value>The inner boundary.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-06-12    
        /// /// </remarks>
        public DateTimePeriod InnerBoundary
        {
            get
            {
               return new DateTimePeriod(
                    _anchor.AddMinutes(-_targetLength.TotalMinutes / 2d).AddMinutes(_flexibility.TotalMinutes),
                    _anchor.AddMinutes(_targetLength.TotalMinutes / 2d).AddMinutes(-_flexibility.TotalMinutes));
            }
        }

    	///<summary>
		/// Gets the payrollcode
    	///</summary>
    	public string PayrollCode
    	{
			get { return _payrollCode; }
    	}

    	#region IEquatable<TimePeriod> Implementation

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(DayOff other)
        {
            return other._anchor == _anchor &&
                   other._targetLength == _targetLength &&
                   other._flexibility == _flexibility &&
				   other._payrollCode == _payrollCode;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if obj and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is DayOff))
            {
                return false;
            }
            return Equals((DayOff) obj);
        }

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator ==(DayOff per1, DayOff per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator !=(DayOff per1, DayOff per2)
        {
            return !per1.Equals(per2);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
			return _anchor.GetHashCode() ^ _targetLength.GetHashCode() ^ _flexibility.GetHashCode();
        }

        #endregion
    }
}