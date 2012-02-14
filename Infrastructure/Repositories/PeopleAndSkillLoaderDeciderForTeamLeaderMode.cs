using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PeopleAndSkillLoaderDeciderForTeamLeaderMode : IPeopleAndSkillLoaderDecider
    {
        public void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> people)
        {
            IList<Guid> list = new List<Guid>();
            PeopleGuidDependencies = list;
            SkillGuidDependencies = list;
            SiteGuidDependencies = list;
        }

        public IEnumerable<Guid> PeopleGuidDependencies { get; protected set; }

        public IEnumerable<Guid> SkillGuidDependencies { get; protected set; }

        public IEnumerable<Guid> SiteGuidDependencies { get; protected set; }

        public double PercentageOfPeopleFiltered
        {
            get { return 0; }
        }

        public int FilterPeople(ICollection<IPerson> people)
        {
            return 0;
        }

        public int FilterSkills(ICollection<ISkill> skills)
        {
            return 0;
        }
    }
}