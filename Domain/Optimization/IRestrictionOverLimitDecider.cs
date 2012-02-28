using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Decides if the otimization is over the given limit by the restriction
    /// </summary>
    public interface IRestrictionOverLimitDecider
    {
        bool PreferencesOverLimit(double limit);
        bool MustHavesOverLimit(double limit);
        bool RotationOverLimit(double limit);
        bool AvailabilitiesOverLimit(double limit);
        bool StudentAvailabilitiesOverLimit(double limit);
    }
}
