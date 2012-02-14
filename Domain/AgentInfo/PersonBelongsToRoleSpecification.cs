using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Specification to filter Person on Site.
    /// </summary>
    public class PersonBelongsToRoleSpecification : Specification<IPerson>
    {
        private readonly IApplicationRole _role;
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToRoleSpecification"/> class.
        /// </summary>
        /// <param name="role">The role.</param>
        public PersonBelongsToRoleSpecification(IApplicationRole role)
        {
            _role = role;
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if the Person satisfies the specification; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSatisfiedBy(IPerson obj)
        {
            if(obj.PermissionInformation == null || obj.PermissionInformation.ApplicationRoleCollection == null)
                return false;
            foreach (ApplicationRole grantedRole in obj.PermissionInformation.ApplicationRoleCollection)
            {
                if (grantedRole == _role)
                    return true;
            }
            return false;
        }
    }
}