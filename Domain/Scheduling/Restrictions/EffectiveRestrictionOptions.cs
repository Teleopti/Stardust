using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionOptions : IEffectiveRestrictionOptions, IEquatable<EffectiveRestrictionOptions>
	{
		public static EffectiveRestrictionOptions UseAll()
		{
			return new EffectiveRestrictionOptions
				{
					UseAvailability = true,
					UsePreference = true,
					//UseStudentAvailability = true
				};
		}

		public bool UsePreference { get; set; }
		public bool UseAvailability { get; set; }
		public bool UseStudentAvailability { get; set; }


		public bool Equals(EffectiveRestrictionOptions other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return UsePreference.Equals(other.UsePreference) && UseAvailability.Equals(other.UseAvailability);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((EffectiveRestrictionOptions) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = UsePreference.GetHashCode();
				hashCode = (hashCode*397) ^ UseAvailability.GetHashCode();
				return hashCode;
			}
		}




	}
}