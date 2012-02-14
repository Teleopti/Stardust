using Teleopti.Ccc.AgentPortalCode.AgentPreference;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView
{
    public class NotValidatedSpecification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool IsSatisfiedBy(IPreferenceCellData obj)
        {
            return (!obj.HasDayOff && !obj.HasAbsence && obj.EffectiveRestriction == null);
        }
    }
}
