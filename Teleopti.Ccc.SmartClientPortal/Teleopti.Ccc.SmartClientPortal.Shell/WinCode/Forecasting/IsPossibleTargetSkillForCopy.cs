using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class IsPossibleTargetSkillForCopy : Specification<ISkill>
    {
        private readonly ISkill _sourceSkill;

        public IsPossibleTargetSkillForCopy(ISkill sourceSkill)
        {
            _sourceSkill = sourceSkill;
        }

        public override bool IsSatisfiedBy(ISkill obj)
        {
            return obj != null && 
                   !(obj is IChildSkill) &&
                   obj.DefaultResolution == _sourceSkill.DefaultResolution &&
                   obj.MidnightBreakOffset == _sourceSkill.MidnightBreakOffset &&
                   obj.TimeZone.Id == _sourceSkill.TimeZone.Id &&
                   obj.SkillType.ForecastSource == _sourceSkill.SkillType.ForecastSource;
        }
    }
}