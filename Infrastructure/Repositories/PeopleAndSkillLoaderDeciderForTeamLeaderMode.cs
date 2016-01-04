using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PeopleAndSkillLoaderDeciderForTeamLeaderMode : IPeopleAndSkillLoaderDecider
    {
        public ILoaderDeciderResult Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> people)
        {
            return new emptyLoaderDeciderResult();
        }

	    private class emptyLoaderDeciderResult : ILoaderDeciderResult
	    {
		    public emptyLoaderDeciderResult()
		    {
				var list = new Guid[]{};
				PeopleGuidDependencies = list;
				SkillGuidDependencies = list;
				SiteGuidDependencies = list;
		    }

		    public Guid[] PeopleGuidDependencies { get; private set; }
		    public Guid[] SkillGuidDependencies { get; private set; }
		    public Guid[] SiteGuidDependencies { get; private set; }
		    public int FilterPeople(ICollection<IPerson> people)
		    {
			    return 0;
		    }

			public int FilterSkills(ISkill[] skills, Action<ISkill> removeSkill, Action<ISkill> addSkill)
		    {
			    return 0;
		    }

		    public double PercentageOfPeopleFiltered { get { return 0; } }
	    }
    }
}