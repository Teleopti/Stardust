using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Decides if the otimization is over the given limit by the restriction
    /// </summary>
    public interface IRestrictionOverLimitDecider
    {
		BrokenRestrictionsInfo PreferencesOverLimit(Percent limit, IScheduleMatrixPro matrix);
		BrokenRestrictionsInfo MustHavesOverLimit(Percent limit, IScheduleMatrixPro matrix);
		BrokenRestrictionsInfo RotationOverLimit(Percent limit, IScheduleMatrixPro matrix);
		BrokenRestrictionsInfo AvailabilitiesOverLimit(Percent limit, IScheduleMatrixPro matrix);
		BrokenRestrictionsInfo StudentAvailabilitiesOverLimit(Percent limit, IScheduleMatrixPro matrix);
    }
}
