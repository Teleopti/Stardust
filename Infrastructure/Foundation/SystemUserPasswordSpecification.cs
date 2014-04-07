using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public class SystemUserPasswordSpecification : Specification<string>, ISystemUserPasswordSpecification
    {
        public override bool IsSatisfiedBy(string obj)
        {
            return !string.IsNullOrEmpty(obj) &&
                   obj.Equals(SuperUser.Password);
        }
    }
}