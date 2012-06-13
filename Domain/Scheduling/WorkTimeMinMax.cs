
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public class WorkTimeMinMax : IWorkTimeMinMax
    {
		private HashSet<IPossibleStartEndCategory> _possibleStartEndCategories = new HashSet<IPossibleStartEndCategory>();
 
        public StartTimeLimitation StartTimeLimitation { get; set; }
        public EndTimeLimitation EndTimeLimitation { get; set; }
        public WorkTimeLimitation WorkTimeLimitation { get; set; }

        public IWorkTimeMinMax Combine(IWorkTimeMinMax workTimeMinMax)
        {
            WorkTimeMinMax ret = new WorkTimeMinMax();
            ret.StartTimeLimitation = combineLimitation(StartTimeLimitation, workTimeMinMax.StartTimeLimitation, new StartTimeLimitation());
            ret.EndTimeLimitation = combineLimitation(EndTimeLimitation, workTimeMinMax.EndTimeLimitation, new EndTimeLimitation());
            ret.WorkTimeLimitation = combineLimitation(WorkTimeLimitation, workTimeMinMax.WorkTimeLimitation, new WorkTimeLimitation());
			ret.PossibleStartEndCategories = _possibleStartEndCategories.Concat(workTimeMinMax.PossibleStartEndCategories).ToList();
			
            return ret;
        }

    	public IList<IPossibleStartEndCategory> PossibleStartEndCategories
    	{
			get { return _possibleStartEndCategories.ToList(); }
			set { _possibleStartEndCategories = new HashSet<IPossibleStartEndCategory>(value); }
    	}

    	private static T combineLimitation<T>(T limitation1, T limitation2, T newLimitation) where T : ILimitation
        {
            newLimitation.StartTime = minTimeSpan(limitation1.StartTime, limitation2.StartTime);
            newLimitation.EndTime = maxTimeSpan(limitation1.EndTime, limitation2.EndTime);

            return newLimitation;
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