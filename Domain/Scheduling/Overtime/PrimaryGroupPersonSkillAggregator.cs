using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class PrimaryGroupPersonSkillAggregator : IGroupPersonSkillAggregator
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public PrimaryGroupPersonSkillAggregator(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public IEnumerable<ISkill> AggregatedSkills(IEnumerable<IPerson> groupMembers, DateOnlyPeriod dateOnlyPeriod)
		{
			var ret = new HashSet<ISkill>();

			foreach (var person in groupMembers)
			{
				var personPeriods = person.PersonPeriods(dateOnlyPeriod);
				foreach (var personPeriod in personPeriods)
				{
					foreach (var personSkill in _personalSkillsProvider.PersonSkills(personPeriod))
					{
						ret.Add(personSkill.Skill);
					}
				}
			}

			return ret;
		}
	}
}