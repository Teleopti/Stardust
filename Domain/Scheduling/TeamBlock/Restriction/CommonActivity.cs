using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public class CommonActivity
	{
		public IActivity Activity { get; set; }
		public IList<DateTimePeriod> Periods { get; set; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((CommonActivity)obj);
		}

		protected bool Equals(CommonActivity other)
		{
			return Equals(Activity, other.Activity) && Equals(Periods, other.Periods);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Activity?.GetHashCode() ?? 0) * 397) ^ periodsHashCode();
			}
		}

		private int periodsHashCode()
		{
			int hashCode = 0;
			if (Periods == null) return hashCode;

			foreach (var p in Periods)
			{
				hashCode ^= p.GetHashCode();
			}
			return hashCode;
		}
	}
}
