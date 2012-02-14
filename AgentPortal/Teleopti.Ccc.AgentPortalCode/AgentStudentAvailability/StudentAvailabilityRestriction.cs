using System;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public class StudentAvailabilityRestriction : ICloneable
    {
        public TimeLimitation StartTimeLimitation { get; set; }
        public TimeLimitation EndTimeLimitation { get; set; }

        public string ShortDateTimePeriod
        {
            get
            {
                if (StartTimeLimitation.MinTime == null || EndTimeLimitation.MaxTime == null)
                    return UserTexts.Resources.NA;
                return StartTimeLimitation.StartTimeString + " - " + EndTimeLimitation.EndTimeString;
            }
        }

        public override bool Equals(object obj)
        {
            var studentAvailability = obj as StudentAvailabilityRestriction;
            if (studentAvailability == null) return false;

            return studentAvailability.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            if (StartTimeLimitation != null)
                hashCode = hashCode ^ StartTimeLimitation.GetHashCode();
            if (EndTimeLimitation != null)
                hashCode = hashCode ^ EndTimeLimitation.GetHashCode();

            return hashCode;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}