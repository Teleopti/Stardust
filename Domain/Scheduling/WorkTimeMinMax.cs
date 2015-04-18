using System;
using Teleopti.Interfaces.Domain;

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
            TimeSpan? ret = checkInitial(t1, t2);
            if (ret != TimeSpan.MinValue)
                return ret;

            return TimeSpan.FromTicks(Math.Min(t1.Value.Ticks, t2.Value.Ticks));
        }

        private static TimeSpan? maxTimeSpan(TimeSpan? t1, TimeSpan? t2)
        {
            TimeSpan? ret = checkInitial(t1, t2);
            if (ret != TimeSpan.MinValue)
                return ret;

            return TimeSpan.FromTicks(Math.Max(t1.Value.Ticks, t2.Value.Ticks));
        }

        private static TimeSpan? checkInitial(TimeSpan? t1, TimeSpan? t2)
        {
            if (!t1.HasValue && !t2.HasValue)
                return null;

            if (t1.HasValue && !t2.HasValue)
                return t1;

            if (!t1.HasValue)
                return t2;

            return TimeSpan.MinValue;
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