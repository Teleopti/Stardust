using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    public class PersonIsUserSpecification : Specification<IPerson>
    {
	    private readonly DateOnly _queryDate;

        public PersonIsUserSpecification(DateOnly queryDate)
        {
            _queryDate = queryDate;
        }

        public override bool IsSatisfiedBy(IPerson obj)
        {
            return !obj.IsAgent(_queryDate) && obj.TerminalDate.GetValueOrDefault(DateOnly.MaxValue) >= _queryDate;
        }
    }
}