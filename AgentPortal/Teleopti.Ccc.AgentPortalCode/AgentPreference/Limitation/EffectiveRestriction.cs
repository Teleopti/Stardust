using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation
{
    public class EffectiveRestriction
    {
        public EffectiveRestriction()
        {
            Invalid = true;
        }

        public EffectiveRestriction(TimeLimitation startTimeLimitation, TimeLimitation endTimeLimitation,
                                    TimeLimitation workTimeLimitation)
        {
            StartTimeLimitation = startTimeLimitation;
            EndTimeLimitation = endTimeLimitation;
            WorkTimeLimitation = workTimeLimitation;
        }

        public TimeLimitation StartTimeLimitation { get; private set; }
        public TimeLimitation EndTimeLimitation { get; private set; }
        public TimeLimitation WorkTimeLimitation { get; private set; }
        public bool Invalid { get; private set; }
    }
}