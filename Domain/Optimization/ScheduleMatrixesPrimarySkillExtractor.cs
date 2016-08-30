using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ScheduleMatrixesPrimarySkillExtractor : ISkillExtractor
	{
		private readonly IPersonalSkillsProvider _personalSkillsProvider;
		private readonly IList<IScheduleMatrixPro> _allScheduleMatrixPros;

		public ScheduleMatrixesPrimarySkillExtractor(IList<IScheduleMatrixPro> allScheduleMatrixPros, IPersonalSkillsProvider personalSkillsProvider)
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