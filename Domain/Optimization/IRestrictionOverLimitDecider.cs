using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Decides if the otimization is over the given limit by the restriction
    /// </summary>
    public interface IRestrictionOverLimitDecider
    {
        IList<DateOnly> PreferencesOverLimit(Percent limit);
        IList<DateOnly> MustHavesOverLimit(Percent limit);
        IList<DateOnly> RotationOverLimit(Percent limit);
        IList<DateOnly> AvailabilitiesOverLimit(Percent limit);
        IList<DateOnly> StudentAvailabilitiesOverLimit(Percent limit);
    }
}
