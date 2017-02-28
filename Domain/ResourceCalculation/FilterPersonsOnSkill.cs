using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class FilterPersonsOnSkill
	{
		public static IList<IPerson> Filter(DateOnly onDate, IEnumerable<IPerson> persons, ISkill filterOnSkill)
		{
			IList<IPerson> ret = new List<IPerson>();
			var period = new DateOnlyPeriod(onDate, onDate.AddDays(1));
			foreach (IPerson person in persons)
			{
				bool found = false;
				foreach (IPersonPeriod personPeriod in person.PersonPeriods(period))
				{
					foreach (IPersonSkill skill in personPeriod.PersonSkillCollection)
					{
						if (skill.Skill.Equals(filterOnSkill) && !ret.Contains(person))
						{
							ret.Add(person);
							found = true;
							break;
						}
					}
					if (found)
						break;
				}
			}
			return ret;
		}

		public static IList<IPerson> Filter(DateOnly onDate, IEnumerable<IPerson> persons, IEnumerable<ISkill> filterOnSkills)
		{
			IList<IPerson> ret = new List<IPerson>();
			var period = new DateOnlyPeriod(onDate, onDate.AddDays(1));
			foreach (IPerson person in persons)
			{
				bool found = false;
				foreach (IPersonPeriod personPeriod in person.PersonPeriods(period))
				{
					foreach (IPersonSkill skill in personPeriod.PersonSkillCollection)
					{
						if (filterOnSkills.Contains(skill.Skill) && !ret.Contains(person))
						{
							ret.Add(person);
							found = true;
							break;
						}
					}
					if (found)
						break;
				}
			}
			return ret;
		}
	}
}
