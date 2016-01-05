using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class LoaderDeciderResult : ILoaderDeciderResult
	{
		private readonly DateTimePeriod _period;
		private double _percentageOfPeopleFiltererd;

		public LoaderDeciderResult(DateTimePeriod period, Guid[] peopleResult, Guid[] skillResult, Guid[] siteResult)
		{
			InParameter.NotNull("peopleResult",peopleResult);
			InParameter.NotNull("skillResult", skillResult);

			_period = period;
			PeopleGuidDependencies = peopleResult;
			SkillGuidDependencies = skillResult;
			SiteGuidDependencies = siteResult;
		}

		public Guid[] PeopleGuidDependencies { get; private set; }

		public Guid[] SkillGuidDependencies { get; private set; }

		public Guid[] SiteGuidDependencies { get; private set; }

		public double PercentageOfPeopleFiltered
		{
			get
			{
				return _percentageOfPeopleFiltererd * 100;
			}
		}

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

			_percentageOfPeopleFiltererd = (people.Count / (double)origCount);

			return origCount - people.Count;
		}

		public int FilterSkills(ISkill[] skills, Action<ISkill> removeSkill, Action<ISkill> addSkill)
		{
			if (SkillGuidDependencies == null)
				throw new InvalidOperationException("Before filtering skills, run Execute method");

			int removed = 0;
			IEnumerable<ISkill> skillsToRemove = (from skill in skills
				where !skill.Id.HasValue || !SkillGuidDependencies.Contains(skill.Id.Value)
				select skill).ToList();
			foreach (ISkill skillToRemove in skillsToRemove)
			{
				removeSkill(skillToRemove);
				removed++;
			}
			foreach (var multisiteSkill in skills.OfType<IChildSkill>().Select(c => c.ParentSkill).Distinct().ToList())
			{
				addSkill(multisiteSkill);
				removed--;
			}

			return removed;
		}
	}
}