namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service Level class
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-06
    /// </remarks>
    public class ServiceLevel : IServiceLevel
    {
        private Percent _percent;
        private double _seconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLevel"/> class.
        /// </summary>
        /// <param name="percent">The percent.</param>
        /// <param name="seconds">The seconds.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-12
        /// </remarks>
        public ServiceLevel(Percent percent, double seconds)
        {
            InParameter.BetweenOneAndHundredPercent("percent", percent);

            _percent = percent;
            _seconds = seconds;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLevel"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-12
        /// </remarks>
        protected ServiceLevel()
        {
        }

        /// <summary>
        /// Gets or sets the percent.
        /// </summary>
        /// <value>The percent.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-06
        /// </remarks>
        public Percent Percent
        {
            get { return _percent;}
            set {
                InParameter.BetweenOneAndHundredPercent("value", value);
                _percent = value;}
        }

        /// <summary>
        /// Gets or sets the seconds.
        /// </summary>
        /// <value>The seconds.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-06
        /// </remarks>
        public double Seconds
        {
            get { return _seconds;}
            set { _seconds = value; }
        }

        #region IEquatable<ServiceLevel> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public bool Equals(IServiceLevel other)
        {
            return other.Percent == _percent &&
                   other.Seconds == _seconds;
        }


        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ServiceLevel))
            {
                return false;
            }
            return Equals((ServiceLevel)obj);
        }


        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public override int GetHashCode()
        {
            return _percent.GetHashCode() ^ _seconds.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="sl1">The SL1.</param>
        /// <param name="sl2">The SL2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public static bool operator ==(ServiceLevel sl1, ServiceLevel sl2)
        {
            return sl1.Equals(sl2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="sl1">The SL1.</param>
        /// <param name="sl2">The SL2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public static bool operator !=(ServiceLevel sl1, ServiceLevel sl2)
        {
            return !sl1.Equals(sl2);
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}