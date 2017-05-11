using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ScheduleMatrixesPrimarySkillExtractor : ISkillExtractor
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;
		private readonly IEnumerable<IScheduleMatrixPro> _allScheduleMatrixPros;

		public ScheduleMatrixesPrimarySkillExtractor(IEnumerable<IScheduleMatrixPro> allScheduleMatrixPros, PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
			_allScheduleMatrixPros = allScheduleMatrixPros;
		}

		public IEnumerable<ISkill> ExtractSkills()
		{
			var skills = new HashSet<ISkill>();

			foreach (var scheduleMatrix in _allScheduleMatrixPros)
			{
				var firstPeriodDay = scheduleMatrix.EffectivePeriodDays[0].Day;
				var personalSkills = _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(scheduleMatrix.Person.Period(firstPeriodDay));

				foreach (var personalSkill in personalSkills)
				{
					skills.Add(personalSkill.Skill);
				}
			}

			return skills;
		}
	}
}