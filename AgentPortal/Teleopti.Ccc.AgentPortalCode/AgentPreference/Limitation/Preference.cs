using System;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation
{
    public class Preference : ICloneable
    {
        public TimeLimitation StartTimeLimitation { get; set; }
        public TimeLimitation EndTimeLimitation { get; set; }
        public TimeLimitation WorkTimeLimitation { get; set; }
        public bool MustHave { get; set; }
        public ShiftCategory ShiftCategory { get; set; }
        public DayOff DayOff { get; set; }
        public Absence Absence { get; set; }
        public string TemplateName { get; set; }

        public Activity Activity { get; set; }
        public TimeLimitation ActivityStartTimeLimitation { get; set; }
        public TimeLimitation ActivityEndTimeLimitation { get; set; }
        public TimeLimitation ActivityTimeLimitation { get; set; }

        public override bool Equals(object obj)
        {
            var preference = obj as Preference;
            if (preference == null) return false;

            return preference.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (ShiftCategory != null)
                hashCode = hashCode ^ ShiftCategory.Id.GetHashCode();
            if (DayOff != null)
                hashCode = hashCode ^ DayOff.Id.GetHashCode();
            if (TemplateName != null)
                hashCode = hashCode ^ TemplateName.GetHashCode();
            if (StartTimeLimitation != null)
                hashCode = hashCode ^ StartTimeLimitation.GetHashCode();
            if (EndTimeLimitation != null)
                hashCode = hashCode ^ EndTimeLimitation.GetHashCode();
            if (WorkTimeLimitation != null)
                hashCode = hashCode ^ WorkTimeLimitation.GetHashCode();
			if (ActivityStartTimeLimitation!= null)
				hashCode = hashCode ^ ActivityStartTimeLimitation.GetHashCode();
			if (ActivityEndTimeLimitation != null)
				hashCode = hashCode ^ ActivityEndTimeLimitation.GetHashCode();
			if (ActivityTimeLimitation != null)
				hashCode = hashCode ^ ActivityTimeLimitation.GetHashCode();

            return hashCode;
        }

        public object Clone()
        {
            var clone = (Preference)MemberwiseClone();
			if (StartTimeLimitation!=null)
        		clone.StartTimeLimitation = (TimeLimitation) StartTimeLimitation.Clone();
			if (EndTimeLimitation != null)
        		clone.EndTimeLimitation = (TimeLimitation) EndTimeLimitation.Clone();
			if (WorkTimeLimitation != null)
        		clone.WorkTimeLimitation = (TimeLimitation) WorkTimeLimitation.Clone();
			if (ActivityStartTimeLimitation != null)
        		clone.ActivityStartTimeLimitation = (TimeLimitation) ActivityStartTimeLimitation.Clone();
			if (ActivityEndTimeLimitation != null)
        		clone.ActivityEndTimeLimitation = (TimeLimitation) ActivityEndTimeLimitation.Clone();
			if (ActivityTimeLimitation != null)
        		clone.ActivityTimeLimitation = (TimeLimitation) ActivityTimeLimitation.Clone();
        	return clone;
        }

    }
}