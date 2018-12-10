using System;

namespace Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence
{
	public class ApprovedPeriod
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }


		protected bool Equals(ApprovedPeriod other)
		{
			return PersonId.Equals(other.PersonId) && StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ApprovedPeriod) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = PersonId.GetHashCode();
				hashCode = (hashCode * 397) ^ StartTime.GetHashCode();
				hashCode = (hashCode * 397) ^ EndTime.GetHashCode();
				return hashCode;
			}
		}
	}
}