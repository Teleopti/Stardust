using System;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Security
{
    public class IsPasswordEncryptedSpecification : Specification<string>
    {
        public override bool IsSatisfiedBy(string obj)
        {
            return !string.IsNullOrEmpty(obj) &&
                   obj.Length > 20 &&
                   obj.StartsWith("###",StringComparison.InvariantCultureIgnoreCase) &&
                   obj.EndsWith("###", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
