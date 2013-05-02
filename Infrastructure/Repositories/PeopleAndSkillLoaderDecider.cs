using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PeopleAndSkillLoaderDecider : IPeopleAndSkillLoaderDecider
    {
        private readonly IPersonRepository _personRepository;
        private readonly IPairMatrixService<Guid> _matrixService;
        private double _percentageOfPeopleFiltererd;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		protected DateTimePeriod Period;

	    public PeopleAndSkillLoaderDecider(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
            _matrixService = new PairMatrixService<Guid>(new PairDictionaryFactory<Guid>());
        }

        public IEnumerable<Guid> PeopleGuidDependencies { get; protected set; }

        public IEnumerable<Guid> SkillGuidDependencies { get; protected set; }

        public IEnumerable<Guid> SiteGuidDependencies { get; protected set; }

        public virtual IPairMatrixService<Guid> MatrixService
        {
            get { return _matrixService; }
        }

        public double PercentageOfPeopleFiltered
        {
            get
            {
                if (PeopleGuidDependencies == null)
                    throw new InvalidOperationException("Before accessing property PercentageOfPeopleFiltered, run Execute and then FilterPeople methods.");

                return _percentageOfPeopleFiltererd * 100;
            }
        }

        public void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> people)
        {
            var matrix = _personRepository.PeopleSkillMatrix(scenario, period);
            IList<Guid> peopleGuids = new List<Guid>();
            people.ForEach(person => peopleGuids.Add(person.Id.Value));
            MatrixService.CreateDependencies(matrix, peopleGuids);
            PeopleGuidDependencies = MatrixService.FirstDependencies;
            SkillGuidDependencies = MatrixService.SecondDependencies;
            SiteGuidDependencies = _personRepository.PeopleSiteMatrix(period);
	        Period = period;
        }

        public int FilterPeople(ICollection<IPerson> people)
        {
            if (PeopleGuidDependencies == null)
                throw new InvalidOperationException("Before filtering people, run Execute method.");

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

			HashSet<ISite> siteList = new HashSet<ISite>();
	        foreach (var person in people)
	        {
				//person.
		        var personPeriods = person.PersonPeriods(Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()));
		        foreach (var personPeriod in personPeriods)
		        {
			        ISite site = personPeriod.Team.Site;
					if (site.MaxSeats.HasValue)
			        {
						siteList.Add(site);
			        }
		        }
	        }

            //Lägg på alla personer som är på samma site om siten har maxseats
            foreach (IPerson personToAdd in peopleToAdd)
            {
                if (people.Contains(personToAdd))
					continue;

				HashSet<ISite> personSiteList = new HashSet<ISite>();
				var personPeriods = personToAdd.PersonPeriods(Period.ToDateOnlyPeriod(personToAdd.PermissionInformation.DefaultTimeZone()));
				foreach (var personPeriod in personPeriods)
				{
					ISite site = personPeriod.Team.Site;
					if (site.MaxSeats.HasValue)
					{
						personSiteList.Add(site);
					}
				}

	            bool found = false;
	            foreach (var site in personSiteList)
	            {
		            if (siteList.Contains(site))
		            {
			            found = true;
		            }
	            }

	            if (found)
                {
					people.Add(personToAdd);
                }
            }

            _percentageOfPeopleFiltererd = (people.Count / (double)origCount);

            return origCount - people.Count;
        }

        public int FilterSkills(ICollection<ISkill> skills)
        {
            if (SkillGuidDependencies == null)
                throw new InvalidOperationException("Before filtering skills, run Execute method");

            int orgCount = skills.Count;
            IEnumerable<ISkill> skillsToRemove = (from skill in skills
                                                  where !skill.Id.HasValue || !SkillGuidDependencies.Contains(skill.Id.Value)
                                                  select skill).ToList();
            foreach (ISkill skillToRemove in skillsToRemove)
            {
                skills.Remove(skillToRemove);
            }
            foreach (IChildSkill child in skills.OfType<IChildSkill>().ToList())
            {
				if (!skills.Contains(child.ParentSkill))
					skills.Add(child.ParentSkill);
            }

            return orgCount - skills.Count;
        }
    }
}
