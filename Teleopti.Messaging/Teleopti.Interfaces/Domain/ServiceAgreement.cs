namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// This class will hold all data for the erlang calculation regarding service level
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-01
    /// Renamed by: Micke 2008-01-08
    /// </remarks>
    public struct ServiceAgreement
    {
    	private IServiceLevel _serviceLevel;
        private Percent _minOccupancy;
        private Percent _maxOccupancy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAgreement"/> struct.
        /// </summary>
        /// <param name="serviceLevel">The service level.</param>
        /// <param name="minOccupancy">The min occupancy.</param>
        /// <param name="maxOccupancy">The max occupancy.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-09
        /// </remarks>
        public ServiceAgreement(IServiceLevel serviceLevel, Percent minOccupancy, Percent maxOccupancy)
        {
            _serviceLevel = serviceLevel;
            _minOccupancy = minOccupancy;
            _maxOccupancy = maxOccupancy;
        }

        /// <summary>
        /// Gets or sets the service level.
        /// </summary>
        /// <value>The service level.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-06
        /// </remarks>
        public IServiceLevel ServiceLevel
        {
            get { return _serviceLevel; }
            set { _serviceLevel = value; }
        }

        /// <summary>
        /// Gets or sets the min occupancy.
        /// </summary>
        /// <value>The min occupancy.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        public Percent MinOccupancy
        {
            get { return _minOccupancy; }
            set { _minOccupancy = value; }
        }

        /// <summary>
        /// Gets or sets the max occupancy.
        /// </summary>
        /// <value>The max occupancy.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-08
        /// </remarks>
        public Percent MaxOccupancy
        {
            get { return _maxOccupancy; }
            set { _maxOccupancy = value; }
        }

        /// <summary>
        /// Returning a service agreement with default values.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-09
        /// </remarks>
        public static ServiceAgreement DefaultValues()
        {
            //hard coded defaults for now, should probably be configurable
            ServiceAgreement serviceAgreement = new ServiceAgreement();
            serviceAgreement.ServiceLevel = new ServiceLevel(new Percent(0.8), 20);
            serviceAgreement.MinOccupancy = new Percent(0.3);
            serviceAgreement.MaxOccupancy = new Percent(0.9);
            return serviceAgreement;
        }
        /// <summary>
        /// Returning a service agreement with default values for email.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-11
        /// </remarks>
        public static ServiceAgreement DefaultValuesEmail()
        {
            //hard coded defaults for now, should probably be configurable
            ServiceAgreement serviceAgreement = new ServiceAgreement();
            serviceAgreement.ServiceLevel = new ServiceLevel(new Percent(1), 7200);
            return serviceAgreement;
        }

        #region IEquatable<ServiceAgreement> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(ServiceAgreement other)
        {
            return other.MinOccupancy == _minOccupancy &&
                   other.MaxOccupancy == _maxOccupancy &&
                   other.ServiceLevel == _serviceLevel;
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
        /// Created date: 2007-11-16
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ServiceAgreement))
            {
                return false;
            }
            return Equals((ServiceAgreement)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public override int GetHashCode()
        {
            return (string.Concat(
                GetType().FullName, "|",
                _minOccupancy, "|" ,
                _maxOccupancy, "|" ,
                _serviceLevel)).GetHashCode();
        }
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="sk1">The SK1.</param>
        /// <param name="sk2">The SK2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public static bool operator ==(ServiceAgreement sk1, ServiceAgreement sk2)
        {
            return sk1.Equals(sk2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="sk1">The SK1.</param>
        /// <param name="sk2">The SK2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public static bool operator !=(ServiceAgreement sk1, ServiceAgreement sk2)
        {
            return !sk1.Equals(sk2);
        }

        #endregion

    }
}