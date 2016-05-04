using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
					foreach (var personSkill in personPeriod.PersonSkillCollection.Where(s => s.Active && !((IDeleteTag)s.Skill).IsDeleted))
					{
						ret.Add(personSkill.Skill );
					}
				}
			}

			return ret;
		}
	}
}