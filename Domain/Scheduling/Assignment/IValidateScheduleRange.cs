using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public interface IValidateScheduleRange
    {
        void ValidateBusinessRules(INewBusinessRuleCollection newBusinessRuleCollection);
    }
}