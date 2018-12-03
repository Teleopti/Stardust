using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IGroupPersonSkillAggregator
	{
		IEnumerable<ISkill> AggregatedSkills(IEnumerable<IPerson> groupMembers, DateOnlyPeriod dateOnlyPeriod);
	}

	public class GroupPersonSkillAggregator : IGroupPersonSkillAggregator
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public GroupPersonSkillAggregator(PersonalSkillsProvider personalSkillsProvider)
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
					foreach (var personSkill in _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(personPeriod))
					{
						ret.Add(personSkill.Skill);
					}
				}
			}

			return ret;
		}
	}
}