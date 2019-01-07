using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class LoaderDeciderResult : ILoaderDeciderResult
	{
		private readonly DateTimePeriod _period;

		public LoaderDeciderResult(DateTimePeriod period, Guid[] peopleResult, Guid[] skillResult, Guid[] siteResult)
		{
			InParameter.NotNull(nameof(peopleResult),peopleResult);
			InParameter.NotNull(nameof(skillResult), skillResult);

			_period = period;
			PeopleGuidDependencies = peopleResult;
			SkillGuidDependencies = skillResult;
			SiteGuidDependencies = siteResult;
		}

		public Guid[] PeopleGuidDependencies { get; }

		public Guid[] SkillGuidDependencies { get; }

		public Guid[] SiteGuidDependencies { get; }

		public int FilterPeople(ICollection<IPerson> people)
		{
			int origCount = people.Count;

			IEnumerable<IPerson> peopleToRemove = (from person in people
				where !person.Id.HasValue || !PeopleGuidDependencies.Contains(person.Id.Value)
				select person).ToList();

			IEnumerable<IPerson> peopleToAdd = (from person in people
				where person.Id.HasValue && SiteGuidDependencies.Contains(person.Id.Value)
				select person).ToList();

			foreach (IPerson personToRemove in peopleToRemove)
			{
				people.Remove(personToRemove);
			}

			var siteList = new HashSet<ISite>();
			foreach (var person in people)
			{
				var maxSeatSites =
					person.PersonPeriods(_period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()))
						.Select(pp => pp.Team.Site)
						.Where(s => s.MaxSeats.HasValue).ToArray();
				foreach (var maxSeatSite in maxSeatSites)
				{
					siteList.Add(maxSeatSite);
				}
			}

			//Lägg på alla personer som är på samma site om siten har maxseats
			foreach (IPerson personToAdd in peopleToAdd)
			{
				if (people.Contains(personToAdd)) continue;

				var personPeriods = personToAdd.PersonPeriods(_period.ToDateOnlyPeriod(personToAdd.PermissionInformation.DefaultTimeZone()));
				var maxSeatSites = personPeriods.Select(pp => pp.Team.Site).Where(s => s.MaxSeats.HasValue).ToArray();
				if (maxSeatSites.Any(siteList.Contains))
				{
					people.Add(personToAdd);
				}
			}

			return origCount - people.Count;
		}

		public int FilterSkills(IEnumerable<ISkill> skills, Action<ISkill> removeSkill, Action<ISkill> addSkill)
		{
			if (SkillGuidDependencies == null)
				throw new InvalidOperationException("Before filtering skills, run Execute method");

			int removed = 0;
			IEnumerable<ISkill> skillsToRemove = (from skill in skills
				where !skill.Id.HasValue || !SkillGuidDependencies.Contains(skill.Id.Value)
				select skill).ToArray();
			foreach (ISkill skillToRemove in skillsToRemove)
			{
				removeSkill(skillToRemove);
				removed++;
			}
			foreach (var multisiteSkill in skills.Except(skillsToRemove).OfType<IChildSkill>().Select(c => c.ParentSkill).Distinct().ToArray())
			{
				addSkill(multisiteSkill);
				removed--;
			}

			return removed;
		}
	}
}