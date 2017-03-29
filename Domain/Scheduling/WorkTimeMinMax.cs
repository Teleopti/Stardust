using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class WorkTimeMinMax : IWorkTimeMinMax
    {
		public StartTimeLimitation StartTimeLimitation { get; set; }
        public EndTimeLimitation EndTimeLimitation { get; set; }
        public WorkTimeLimitation WorkTimeLimitation { get; set; }

        public IWorkTimeMinMax Combine(IWorkTimeMinMax workTimeMinMax)
        {
        	return new WorkTimeMinMax
        	          	{
        	          		StartTimeLimitation = new StartTimeLimitation(
        	          			minTimeSpan(StartTimeLimitation.StartTime, workTimeMinMax.StartTimeLimitation.StartTime),
        	          			maxTimeSpan(StartTimeLimitation.EndTime, workTimeMinMax.StartTimeLimitation.EndTime)),
        	          		EndTimeLimitation = new EndTimeLimitation(
        	          			minTimeSpan(EndTimeLimitation.StartTime, workTimeMinMax.EndTimeLimitation.StartTime),
        	          			maxTimeSpan(EndTimeLimitation.EndTime, workTimeMinMax.EndTimeLimitation.EndTime)),
        	          		WorkTimeLimitation = new WorkTimeLimitation(
        	          			minTimeSpan(WorkTimeLimitation.StartTime, workTimeMinMax.WorkTimeLimitation.StartTime),
        	          			maxTimeSpan(WorkTimeLimitation.EndTime, workTimeMinMax.WorkTimeLimitation.EndTime)),
        	          	};
        }

        private static TimeSpan? minTimeSpan(TimeSpan? t1, TimeSpan? t2)
        {
	        if (!t1.HasValue && !t2.HasValue) return null;

	        var span1 = t1.GetValueOrDefault(TimeSpan.MaxValue);
	        var span2 = t2.GetValueOrDefault(TimeSpan.MaxValue);

	        return span1 < span2 ? span1 : span2;
        }

        private static TimeSpan? maxTimeSpan(TimeSpan? t1, TimeSpan? t2)
        {
			if (!t1.HasValue && !t2.HasValue) return null;

			var span1 = t1.GetValueOrDefault(TimeSpan.MinValue);
			var span2 = t2.GetValueOrDefault(TimeSpan.MinValue);

			return span1 > span2 ? span1 : span2;
		}
		
    	public bool Equals(WorkTimeMinMax other)
    	{
    		if (ReferenceEquals(null, other)) return false;
    		if (ReferenceEquals(this, other)) return true;
    		return other.StartTimeLimitation.Equals(StartTimeLimitation) && other.EndTimeLimitation.Equals(EndTimeLimitation) && other.WorkTimeLimitation.Equals(WorkTimeLimitation);
    	}

    	public override bool Equals(object obj)
    	{
    		if (ReferenceEquals(null, obj)) return false;
    		if (ReferenceEquals(this, obj)) return true;
    		if (obj.GetType() != typeof (WorkTimeMinMax)) return false;
    		return Equals((WorkTimeMinMax) obj);
    	}

    	public override int GetHashCode()
    	{
    		unchecked
    		{
    			int result = StartTimeLimitation.GetHashCode();
    			result = (result*397) ^ EndTimeLimitation.GetHashCode();
    			result = (result*397) ^ WorkTimeLimitation.GetHashCode();
    			return result;
    		}
    	}

    	public static bool operator ==(WorkTimeMinMax left, WorkTimeMinMax right)
    	{
    		return Equals(left, right);
    	}

    	public static bool operator !=(WorkTimeMinMax left, WorkTimeMinMax right)
    	{
    		return !Equals(left, right);
    	}
    }
}