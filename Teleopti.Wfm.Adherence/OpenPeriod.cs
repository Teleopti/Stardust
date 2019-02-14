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

		public bool Intersects(OpenPeriod period)
		{
			var startsAfterPeriodEnds = StartTime > period.EndTime;
			var endsBeforePeriodStarts = EndTime < period.StartTime;
			return !(startsAfterPeriodEnds || endsBeforePeriodStarts); 
		}

		public bool StartsBefore(OpenPeriod period)
		{
			if (StartTime == null && period.StartTime != null)
				return true;
			return StartTime < period.StartTime;
		}

		public bool EndsAfter(OpenPeriod period)
		{
			if (EndTime == null && period.EndTime != null)
				return true;			
			return EndTime > period.EndTime;
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