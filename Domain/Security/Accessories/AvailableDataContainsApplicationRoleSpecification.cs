using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Accessories
{
    /// <summary>
    /// Specification to filter out Matrix reports.
    /// </summary>
    public class AvailableDataContainsApplicationRoleSpecification : Specification<IAvailableData>
    {
        private readonly IEnumerable<IApplicationRole> _applicationRoles;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableDataContainsApplicationRoleSpecification"/> class.
        /// </summary>
        /// <param name="applicationRoles">The list of application roles.</param>
        public AvailableDataContainsApplicationRoleSpecification(IEnumerable<IApplicationRole> applicationRoles)
        {
            InParameter.NotNull("applicationRoles", applicationRoles);
            _applicationRoles = applicationRoles;
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        public override bool IsSatisfiedBy(IAvailableData obj)
        {
            if (obj == null || obj.ApplicationRole == null)
                return false;

            return (_applicationRoles.Contains(obj.ApplicationRole));
        }
    }
}
