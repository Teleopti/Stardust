using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public interface IValidateScheduleRange
    {
        void ValidateBusinessRules(INewBusinessRuleCollection newBusinessRuleCollection);
    }
}