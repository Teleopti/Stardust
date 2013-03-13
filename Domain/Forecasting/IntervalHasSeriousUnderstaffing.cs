using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasSeriousUnderstaffing : Specification<ISkillStaffPeriod>
    {
        private readonly ISkill _skill;

        public IntervalHasSeriousUnderstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(ISkillStaffPeriod obj)
        {
            return obj.RelativeDifference < _skill.StaffingThresholds.SeriousUnderstaffing.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public string GetSeriousUnderstaffingHours(IEnumerable<ISkillStaffPeriod> skillStaffPeriodList, IPerson requestingAgent)
        {
            var seriousUnderStaffing = "";
            var count = 0;
            var timeZone = requestingAgent.PermissionInformation.DefaultTimeZone();
            var culture = requestingAgent.PermissionInformation.Culture();

            foreach (var skillStaffPeriod in skillStaffPeriodList)
            {
                if (skillStaffPeriod.RelativeDifference < _skill.StaffingThresholds.SeriousUnderstaffing.Value)
                {
                    count++;
                    if (count > 5)
                        break;

                    var startTime =
                        skillStaffPeriod.Period.StartDateTimeLocal(timeZone).ToString("t", culture);
                    var endTime =
                        skillStaffPeriod.Period.EndDateTimeLocal(timeZone).ToString("t", culture);
                   
                    seriousUnderStaffing += startTime + "-" + endTime + ",";
                }

            }

            if(seriousUnderStaffing.Length>1)
                seriousUnderStaffing = seriousUnderStaffing.Substring(0, seriousUnderStaffing.Length - 1);
            return seriousUnderStaffing;
        }
    }
}