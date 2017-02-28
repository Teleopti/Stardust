using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public static class RestrictionMustBe
    {
        public static readonly ISpecification<IRestrictionBase> Availability = new AvailabilitySpecification();
        public static readonly ISpecification<IRestrictionBase> Rotation = new RotationSpecification();
        public static readonly ISpecification<IRestrictionBase> Preference = new PreferenceSpecification();
        public static readonly ISpecification<IRestrictionBase> StudentAvailability = new StudentSpecification();
    }
}