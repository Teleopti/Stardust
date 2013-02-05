

using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IGroupPersonSkillAggregator
	{
		IEnumerable<ISkill> AggregatedSkills(GroupPerson groupPerson, DateOnly minValue);
	}

	public class GroupPersonSkillAggregator : IGroupPersonSkillAggregator
	{
		public IEnumerable<ISkill> AggregatedSkills(GroupPerson groupPerson, DateOnly minValue)
		{
			var ret = new HashSet<ISkill>();
		    if (groupPerson != null)
		        foreach (var person in groupPerson.GroupMembers)
		        {
		            IPersonPeriod personPeriod = person.Period(minValue);
		            if(personPeriod != null)
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