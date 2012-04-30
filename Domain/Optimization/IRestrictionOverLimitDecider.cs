using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Decides if the otimization is over the given limit by the restriction
    /// </summary>
    public interface IRestrictionOverLimitDecider
    {
        BrokenRestrictionsInfo PreferencesOverLimit(Percent limit);
        BrokenRestrictionsInfo MustHavesOverLimit(Percent limit);
        BrokenRestrictionsInfo RotationOverLimit(Percent limit);
        BrokenRestrictionsInfo AvailabilitiesOverLimit(Percent limit);
        BrokenRestrictionsInfo StudentAvailabilitiesOverLimit(Percent limit);
    }
}
