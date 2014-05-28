using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatSkillAggregator
	{
		HashSet<ISkill> GetAggregatedSkills(IList<IPerson> teamMembers, DateOnlyPeriod dateOnlyPeriod);
	}

	public class MaxSeatSkillAggregator : IMaxSeatSkillAggregator
	{
		public HashSet< ISkill> GetAggregatedSkills(IList<IPerson> teamMembers,  DateOnlyPeriod dateOnlyPeriod )
		{
			var ret = new HashSet<ISkill>();

			foreach (var person in teamMembers)
			{
				var personPeriods = person.PersonPeriods(dateOnlyPeriod);
				foreach (var personPeriod in personPeriods)
				{
					foreach (var personSkill in personPeriod.PersonMaxSeatSkillCollection)
					{
						ret.Add(personSkill.Skill);
					}
				}
			}

			return ret;
		}
	}
}