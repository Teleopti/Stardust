using System;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class DayOff : IDayOff
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

        public override bool Equals(object obj)
        {
	        var other = obj as DayOff;
					if (other == null)
					{
						return false;
					}
	        return other._anchor == _anchor &&
	               other._targetLength == _targetLength &&
	               other._flexibility == _flexibility &&
	               other._payrollCode == _payrollCode;
        }

        public override int GetHashCode()
        {
			return _anchor.GetHashCode() ^ _targetLength.GetHashCode() ^ _flexibility.GetHashCode();
        }

    }
}