using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class StudentSpecification : Specification<IRestrictionBase>
    {
        public override bool IsSatisfiedBy(IRestrictionBase obj)
        {
            return obj is IStudentAvailabilityRestriction;
        }
    }
}
