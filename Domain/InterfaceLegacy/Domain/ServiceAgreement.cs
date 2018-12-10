using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public struct ServiceAgreement : IEquatable<ServiceAgreement>
    {
    	private readonly ServiceLevel _serviceLevel;
        private readonly Percent _minOccupancy;
        private readonly Percent _maxOccupancy;
		
        public ServiceAgreement(ServiceLevel serviceLevel, Percent minOccupancy, Percent maxOccupancy)
        {
            _serviceLevel = serviceLevel;
            _minOccupancy = minOccupancy;
            _maxOccupancy = maxOccupancy;
        }
		
        public ServiceLevel ServiceLevel => _serviceLevel;

	    public Percent MinOccupancy => _minOccupancy;

	    public Percent MaxOccupancy => _maxOccupancy;

	    public static ServiceAgreement DefaultValues()
        {
	        return new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20),
		        new Percent(0.3), new Percent(0.9));
        }
		
        public static ServiceAgreement DefaultValuesEmail()
        {
            return new ServiceAgreement(new ServiceLevel(new Percent(1), 7200), new Percent(), new Percent());
        }
		
        public bool Equals(ServiceAgreement other)
        {
            return other.MinOccupancy == _minOccupancy &&
                   other.MaxOccupancy == _maxOccupancy &&
                   other.ServiceLevel == _serviceLevel;
        }
		
        public override bool Equals(object obj)
		{
			return obj is ServiceAgreement agreement && Equals(agreement);
		}
		
        public override int GetHashCode()
        {
            return (string.Concat(
                GetType().FullName, "|",
                _minOccupancy, "|" ,
                _maxOccupancy, "|" ,
                _serviceLevel)).GetHashCode();
        }

        public static bool operator ==(ServiceAgreement sk1, ServiceAgreement sk2)
        {
            return sk1.Equals(sk2);
        }
		
        public static bool operator !=(ServiceAgreement sk1, ServiceAgreement sk2)
        {
            return !sk1.Equals(sk2);
        }

	    public ServiceAgreement WithMinOccupancy(Percent percent)
	    {
		    return new ServiceAgreement(_serviceLevel,percent,_maxOccupancy);
	    }

	    public ServiceAgreement WithMaxOccupancy(Percent percent)
	    {
			return new ServiceAgreement(_serviceLevel, _minOccupancy, percent);
		}

	    public ServiceAgreement WithServiceLevel(ServiceLevel serviceLevel)
	    {
		    return new ServiceAgreement(serviceLevel,_minOccupancy,_maxOccupancy);
	    }
    }
}