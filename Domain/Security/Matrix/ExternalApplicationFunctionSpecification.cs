using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
    /// <summary>
    /// Specification to filter out external (Matrix) functions.
    /// </summary>
    public class ExternalApplicationFunctionSpecification : Specification<IApplicationFunction>
    {
        private string _foreignSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalApplicationFunctionSpecification"/> class.
        /// </summary>
        /// <param name="foreignSource">The foreign source.</param>
        public ExternalApplicationFunctionSpecification(string foreignSource)
        {
            _foreignSource = foreignSource;
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        public override bool IsSatisfiedBy(IApplicationFunction obj)
        {
            return (!string.IsNullOrEmpty(obj.ForeignSource) && obj.ForeignSource == _foreignSource);
        }
    }
}