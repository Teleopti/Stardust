using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public struct ServiceLevel : IEquatable<ServiceLevel>,
									ICloneable
	{
        private readonly Percent _percent;
        private readonly double _seconds;
		
        public ServiceLevel(Percent percent, double seconds)
        {
            InParameter.BetweenOneAndHundredPercent(nameof(percent), percent);

            _percent = percent;
            _seconds = seconds;
        }
		
        public Percent Percent => _percent;

		public double Seconds => _seconds;

		public bool Equals(ServiceLevel other)
        {
            return other.Percent == _percent &&
                   other.Seconds == _seconds;
        }
		
        public override bool Equals(object obj)
		{
			return obj is ServiceLevel level && Equals(level);
		}
		
        public override int GetHashCode()
        {
            return _percent.GetHashCode() ^ _seconds.GetHashCode();
        }
		
        public static bool operator ==(ServiceLevel sl1, ServiceLevel sl2)
        {
            return sl1.Equals(sl2);
        }
		
        public static bool operator !=(ServiceLevel sl1, ServiceLevel sl2)
        {
            return !sl1.Equals(sl2);
        }
		
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}