using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Security
{
    public class RegExpSpecification : Specification<string>
    {
        private readonly string _regexp;

        public RegExpSpecification(string regExp)
        {
            _regexp = regExp;
        }

        public override bool IsSatisfiedBy(string obj)
        {
            Regex regex = new Regex(_regexp);
            return regex.IsMatch(obj);
        }
    }
}
