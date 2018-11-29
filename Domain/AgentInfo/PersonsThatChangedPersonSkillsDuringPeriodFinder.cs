using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PersonsThatChangedPersonSkillsDuringPeriodFinder
	{
		public IList<PersonsThatChangedPersonSkillsDuringPeriodFinderResult> Find(DateOnlyPeriod period,
			IEnumerable<IPerson> personList)
		{
			var result = new List<PersonsThatChangedPersonSkillsDuringPeriodFinderResult>();

			foreach (var person in personList)
			{
				var personPeriod = person.Period(period.StartDate);
				if(personPeriod == null)
					continue;

				var personSkillCollectionForPeriod = personPeriod.PersonSkillCollection;
				var date = personPeriod.Period.EndDate.AddDays(1);
				while (date <= period.EndDate)
				{
					var tmpResult = new PersonsThatChangedPersonSkillsDuringPeriodFinderResult(person, date);
					var currentPeriod = person.Period(date);
					var skillCollectionForPeriod = personSkillCollectionForPeriod as IPersonSkill[] ?? personSkillCollectionForPeriod.ToArray();
					foreach (var personSkill in currentPeriod.PersonSkillCollection)
					{
						if(!find(skillCollectionForPeriod, personSkill.Skill))
							tmpResult.AddedSkills.Add(personSkill.Skill);
					}

					foreach (var personSkill in skillCollectionForPeriod)
					{
						if (!find(currentPeriod.PersonSkillCollection, personSkill.Skill))
							tmpResult.RemovedSkills.Add(personSkill.Skill);
					}

					if (tmpResult.AddedSkills.Any() || tmpResult.RemovedSkills.Any())
						result.Add(tmpResult);

					date = currentPeriod.Period.EndDate.AddDays(1);
					personSkillCollectionForPeriod = currentPeriod.PersonSkillCollection;
				}
			}

			return result;
		}

		private static bool find(IEnumerable<IPersonSkill> skillCollection, ISkill skill)
		{
			var found = false;
			foreach (var currentPersonSkill in skillCollection)
			{
				if (currentPersonSkill.Skill.Equals(skill))
					found = true;
			}
			return found;
		}

		public class PersonsThatChangedPersonSkillsDuringPeriodFinderResult
		{
			public PersonsThatChangedPersonSkillsDuringPeriodFinderResult(IPerson person, DateOnly date)
			{
				Person = person;
				Date = date;
				AddedSkills = new List<ISkill>();
				RemovedSkills = new List<ISkill>();
			}

			public IPerson Person { get; private set; }
			public DateOnly Date { get; private set; }
			public IList<ISkill> AddedSkills { get; private set; }
			public IList<ISkill> RemovedSkills { get; private set; }
		}
	}
}