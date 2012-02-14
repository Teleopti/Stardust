using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Accessories
{
    /// <summary>
    /// Specification to filter application roles that contains a specific application function.
    /// </summary>
    public class ApplicationRoleContainsApplicationFunctionSpecification : Specification<IApplicationRole>
    {
        private readonly IApplicationFunction _applicationFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRoleContainsApplicationFunctionSpecification"/> class.
        /// </summary>
        /// <param name="applicationFunction">The application function.</param>
        public ApplicationRoleContainsApplicationFunctionSpecification(IApplicationFunction applicationFunction)
        {
            InParameter.NotNull("applicationFunction", applicationFunction);
            _applicationFunction = applicationFunction;
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        public override bool IsSatisfiedBy(IApplicationRole obj)
        {
            if (obj == null)
                return false;

            return (obj.ApplicationFunctionCollection.Contains(_applicationFunction));
        }
    }
}