using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Extracts the personal skills from a given matrix
    /// </summary>
    public class ScheduleMatrixPersonalSkillExtractor : ISkillExtractor
    {
        private readonly IScheduleMatrixPro _scheduleMatrix;
	    private readonly PersonalSkillsProvider _personalSkillsProvider;

	    public ScheduleMatrixPersonalSkillExtractor(IScheduleMatrixPro scheduleMatrix, PersonalSkillsProvider personalSkillsProvider)
	    {
		    _scheduleMatrix = scheduleMatrix;
		    _personalSkillsProvider = personalSkillsProvider;
	    }

	    public IEnumerable<ISkill> ExtractSkills()
        {
			DateOnly firstPeriodDay = _scheduleMatrix.EffectivePeriodDays[0].Day;
            var personalSkills = _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(_scheduleMatrix.Person.Period(firstPeriodDay));
            return personalSkills.Select(personalSkill => personalSkill.Skill).ToList();
        }
    }
}
