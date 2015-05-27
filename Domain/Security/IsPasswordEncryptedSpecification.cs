using System;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Security
{
	//TODO: tenant Can (probably?) be removed when removing old schema (when PasswordEncryption type is gone)
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
