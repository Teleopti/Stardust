using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasBelowMinAgents : Specification<ISkillStaffPeriod>
    {
        public override bool IsSatisfiedBy(ISkillStaffPeriod obj)
        {
            return obj.Payload.SkillPersonData.MinimumPersons > obj.Payload.CalculatedLoggedOn;
        }
    }
}