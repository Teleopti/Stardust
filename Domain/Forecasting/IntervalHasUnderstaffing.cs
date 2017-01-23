using log4net;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasUnderstaffing : Specification<IValidatePeriod>
    {
        private readonly ISkill _skill;
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntervalHasUnderstaffing));

		public IntervalHasUnderstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(IValidatePeriod obj)
        {
			logger.Info($"IsSatisfiedBy -- {_skill.Name}  {obj.DateTimePeriod} -- rel diff: {obj.RelativeDifference} -- threshold {_skill.StaffingThresholds.Understaffing.Value}");
			return obj.RelativeDifference < _skill.StaffingThresholds.Understaffing.Value;
        }

    }
}