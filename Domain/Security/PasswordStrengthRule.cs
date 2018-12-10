using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Security
{
    public class PasswordStrengthRule:IPasswordStrengthRule
    {
        private readonly int _minimumAcceptedRules;
        private readonly IList<ISpecification<string>> _expressions;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public PasswordStrengthRule(int minimumAcceptedRules, IList<ISpecification<string>> expressions)
        {
            _minimumAcceptedRules = minimumAcceptedRules;
            _expressions = expressions;
        }

        public bool VerifyPasswordStrength(string password)
        {
            return _expressions.Count(e => e.IsSatisfiedBy(password)) >= _minimumAcceptedRules;
        }
    }
}
