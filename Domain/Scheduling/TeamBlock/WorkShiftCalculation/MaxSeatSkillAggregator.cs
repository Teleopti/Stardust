using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface IMaxSeatSkillAggregator
	{
		HashSet<ISkill> GetAggregatedSkills(IList<IPerson> teamMembers, DateOnlyPeriod dateOnlyPeriod);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
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
					if (personPeriod.MaxSeatSkill != null)
					{
						ret.Add(personPeriod.MaxSeatSkill);
					}
				}
			}

			return ret;
		}
	}
}