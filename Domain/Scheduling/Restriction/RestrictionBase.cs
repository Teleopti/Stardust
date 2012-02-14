using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
   
    public class RestrictionBase : AggregateEntity, IRestrictionBase
    {
        private  StartTimeLimitation _startTimeLimitation;
        private  EndTimeLimitation _endTimeLimitation;
        private  WorkTimeLimitation _workTimeLimitation;


        public virtual StartTimeLimitation StartTimeLimitation
        {
            get { return _startTimeLimitation; }
            set { _startTimeLimitation = value; }
        }

        public virtual EndTimeLimitation EndTimeLimitation
        {
            get { return _endTimeLimitation; }
            set { _endTimeLimitation = value; }
        }

        public virtual WorkTimeLimitation WorkTimeLimitation
        {
            get { return _workTimeLimitation; }
            set { _workTimeLimitation = value; }
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public virtual bool IsRestriction()
        {
            if (StartTimeLimitation.EndTime != null || StartTimeLimitation.StartTime != null)
                return true;

            if (EndTimeLimitation.EndTime != null || EndTimeLimitation.StartTime != null)
                return true;

            if (WorkTimeLimitation.EndTime != null || WorkTimeLimitation.StartTime != null)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
                { 
                    var result = 0;
                    result = (result*396) ^ base.GetHashCode();
                    result = (result*396) ^ StartTimeLimitation.GetHashCode();
                    result = (result*396) ^ EndTimeLimitation.GetHashCode();
                    result = (result*396) ^ WorkTimeLimitation.GetHashCode(); 
                    return result; 
                }
        }
    }
}

