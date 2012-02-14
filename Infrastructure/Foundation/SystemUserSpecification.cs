using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Obfuscated.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public class SystemUserSpecification : Specification<IPerson>, ISystemUserSpecification
    {
        public override bool IsSatisfiedBy(IPerson obj)
        {
            return obj != null &&
                   obj.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName == SuperUser.UserName;
        }
    }
}