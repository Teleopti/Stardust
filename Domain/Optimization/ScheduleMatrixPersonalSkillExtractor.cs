using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Extracts the personal skills from a given matrix
    /// </summary>
    public class ScheduleMatrixPersonalSkillExtractor : ISkillExtractor
    {
        private readonly IScheduleMatrixPro _scheduleMatrix;

        public ScheduleMatrixPersonalSkillExtractor(IScheduleMatrixPro scheduleMatrix)
        {
            _scheduleMatrix = scheduleMatrix;
        }

        public IEnumerable<ISkill> ExtractSkills()
        {
            DateOnly firstPeriodDay = _scheduleMatrix.EffectivePeriodDays[0].Day;
            var personalSkills = _scheduleMatrix.Person.Period(firstPeriodDay).PersonSkillCollection;
            return personalSkills.Select(personalSkill => personalSkill.Skill).ToList();
        }
    }
}
