using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class SchedulingResultStateHolderFactory
    {
        /// <summary>
        /// Creates an empty <see cref="SchedulingResultStateHolder"/> the with the given <paramref name="period"/> period
        /// and the given <paramref name="skill"/> skill.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skill">The skill.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static SchedulingResultStateHolder Create(DateTimePeriod period, ISkill skill)
        {

            SchedulingResultStateHolder result = Create(period);
            result.AddSkills(skill);
          
            return result;
        }

        /// <summary>
        /// Creates an empty <see cref="SchedulingResultStateHolder"/> the with the given <paramref name="period"/> period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public static SchedulingResultStateHolder Create(DateTimePeriod period)
        {
            return new SchedulingResultStateHolder(new List<IPerson>(),
                                                   new ScheduleDictionary(new Scenario("test"), new ScheduleDateTimePeriod(period), new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make()),
                                                   new Dictionary<ISkill, IEnumerable<ISkillDay>>());
        }

        /// <summary>
        /// Creates an <see cref="SchedulingResultStateHolder"/> the with the given parameters.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="skillDays">The skill days.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static SchedulingResultStateHolder Create(DateTimePeriod period, ISkill skill, IList<ISkillDay> skillDays)
        {
            SchedulingResultStateHolder result = Create(period, skill);
            result.SkillDays.Add(skill, skillDays);

            return result;

        }
    }
}
