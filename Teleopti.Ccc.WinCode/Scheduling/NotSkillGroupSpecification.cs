using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Specification to filter skill group specification.
    /// </summary>
    public class NotSkillGroupSpecification : Specification<IGroupPage>
    {
        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        public override bool IsSatisfiedBy(IGroupPage obj)
        {
            return obj.DescriptionKey != "Skill";
        }
    }
}