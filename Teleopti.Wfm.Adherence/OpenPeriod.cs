using System;

namespace Teleopti.Wfm.Adherence
{
	public class OpenPeriod
	{
		public DateTime? StartTime;
		public DateTime? EndTime;

		public OpenPeriod()
		{
		}

		public OpenPeriod(DateTime? startTime, DateTime? endTime)
		{
			StartTime = startTime;
			EndTime = endTime;
		}

		protected bool Equals(OpenPeriod other)
		{
			return StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((OpenPeriod) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (StartTime.GetHashCode() * 397) ^ EndTime.GetHashCode();
			}
		}
	}
}