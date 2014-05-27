using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IGroupPersonSkillAggregator
	{
		IEnumerable<ISkill> AggregatedSkills(IEnumerable<IPerson> groupMembers, DateOnlyPeriod dateOnlyPeriod);
	}

	public class GroupPersonSkillAggregator : IGroupPersonSkillAggregator
	{
		public IEnumerable<ISkill> AggregatedSkills(IEnumerable<IPerson> groupMembers, DateOnlyPeriod dateOnlyPeriod)
		{
			var ret = new HashSet<ISkill>();

			foreach (var person in groupMembers)
			{
				var personPeriods = person.PersonPeriods(dateOnlyPeriod);
				foreach (var personPeriod in personPeriods)
				{
					foreach (var personSkill in personPeriod.PersonSkillCollection)
					{
						ret.Add(personSkill.Skill );
					}
				}
			}

			return ret;
		}
	}
}