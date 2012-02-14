using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasAboveMaxAgents : Specification<ISkillStaffPeriod>
    {
        public override bool IsSatisfiedBy(ISkillStaffPeriod obj)
        {
            return obj.Payload.SkillPersonData.MaximumPersons != 0 &&
                   obj.Payload.SkillPersonData.MaximumPersons < obj.Payload.CalculatedLoggedOn;
        }
    }
}