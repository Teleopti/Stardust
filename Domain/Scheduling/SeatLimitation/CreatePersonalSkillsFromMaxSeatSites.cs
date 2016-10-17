using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface ICreatePersonalSkillsFromMaxSeatSites
	{
		void Process(DateOnlyPeriod period, IEnumerable<IPerson> personsInOrganisation);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class CreatePersonalSkillsFromMaxSeatSites : ICreatePersonalSkillsFromMaxSeatSites
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void Process(DateOnlyPeriod period, IEnumerable<IPerson> personsInOrganisation)
		{
            foreach (var person in personsInOrganisation)
			{
				processPerson(period, person);
			}
		}

		private static void processPerson(DateOnlyPeriod period, IPerson person)
		{
			foreach (var dateOnly in period.DayCollection())
			{
				processPersonOnDate(dateOnly, person);
			}
		}

		private static void processPersonOnDate(DateOnly dateOnly, IPerson person)
		{
			IPersonPeriod personPeriod = person.Period(dateOnly);
			if (personPeriod == null)
				return;

			ISkill skill = personPeriod.Team.Site.MaxSeatSkill;
			if (skill == null)
				return;

			bool found = false;
			foreach (var personSkill in personPeriod.PersonMaxSeatSkillCollection)
			{
				if(personSkill.Skill.Equals(skill))
				{
					found = true;
					break;
				}
			}

			if(!found)
			{
				IPersonSkill personSkill = new PersonSkill(skill, new Percent(1));
				personPeriod.AddPersonMaxSeatSkill(personSkill);
			}
		}
	}

	

}