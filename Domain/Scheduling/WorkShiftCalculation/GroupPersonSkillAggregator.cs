using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IGroupPersonSkillAggregator
	{
		IEnumerable<ISkill> AggregatedSkills(IGroupPerson groupPerson, DateOnlyPeriod dateOnlyPeriod);
	}

	public class GroupPersonSkillAggregator : IGroupPersonSkillAggregator
	{
		public IEnumerable<ISkill> AggregatedSkills(IGroupPerson groupPerson, DateOnlyPeriod dateOnlyPeriod)
		{
			var ret = new HashSet<ISkill>();
		    if (groupPerson != null)
		        foreach (var person in groupPerson.GroupMembers)
		        {
					var personPeriods = person.PersonPeriods(dateOnlyPeriod);
			        foreach (var personPeriod in personPeriods)
			        {
						foreach (var personSkill in personPeriod.PersonSkillCollection)
						{
							ret.Add(personSkill.Skill);
						}
			        }
		        }

		    return ret;
		}
	}
}