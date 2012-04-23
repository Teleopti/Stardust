using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionOptions : IEffectiveRestrictionOptions, IEquatable<EffectiveRestrictionOptions>
	{
		public bool UseAvailability { get; set; }
		public bool UsePreference { get; set; }

		public EffectiveRestrictionOptions(bool usePreference, bool useAvailability)
		{
			UseAvailability = useAvailability;
			UsePreference = usePreference;
		}







		public bool Equals(EffectiveRestrictionOptions other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.UseAvailability.Equals(UseAvailability) && other.UsePreference.Equals(UsePreference);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (EffectiveRestrictionOptions)) return false;
			return Equals((EffectiveRestrictionOptions) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (UseAvailability.GetHashCode()*397) ^ UsePreference.GetHashCode();
			}
		}

		public static bool operator ==(EffectiveRestrictionOptions left, EffectiveRestrictionOptions right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(EffectiveRestrictionOptions left, EffectiveRestrictionOptions right)
		{
			return !Equals(left, right);
		}
	}
}