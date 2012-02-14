using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class AffectedPersonSkillService : IAffectedPersonSkillService
    {
        private readonly DateOnlyPeriod _outerPeriod;
        private readonly ICollection<ISkill> _skillCollection;
        private readonly IDictionary<IPerson, IList<PersonSkillPeriod>> statePerPersonForAllActivities;

        public AffectedPersonSkillService(DateOnlyPeriod outerPeriod, ICollection<ISkill> skillCollection)
        {
            InParameter.NotNull("validSkillCollection", skillCollection);
            _outerPeriod = outerPeriod;
            _skillCollection = skillCollection;
            statePerPersonForAllActivities = new Dictionary<IPerson, IList<PersonSkillPeriod>>();
        }

        public ICollection<IPersonSkill> Execute(IPerson person, IActivity activity, DateOnly dateOnly)
        {
            if (!_outerPeriod.Contains(dateOnly))
                throw new ArgumentOutOfRangeException("dateOnly", "Cannot get affected personskills for date outside outer period");

			IEnumerable<IPersonSkill> workingList = findOrAddPersonSkillsInCache(person, dateOnly);

            return filterPersonSkillByActivity(workingList, activity);
        }

        private IEnumerable<IPersonSkill> findOrAddPersonSkillsInCache(IPerson person, DateOnly dateOnly)
        {
        	IList<PersonSkillPeriod> personSkillPeriods;
        	bool cached = statePerPersonForAllActivities.TryGetValue(person, out personSkillPeriods);
            if (!cached)
            {
                IList<IPersonPeriod> personPeriods = person.PersonPeriods(_outerPeriod);
            	personSkillPeriods =
            		personPeriods.Select(
            			p =>
            			new PersonSkillPeriod
            				{
            					Period = p.Period,
            					PersonSkillCollection = p.PersonSkillCollection.Where(s => _skillCollection.Contains(s.Skill)).ToList()
            				}).ToList();
                statePerPersonForAllActivities[person] = personSkillPeriods;
            }

			IList<IPersonSkill> affectedPersonSkills = new List<IPersonSkill>();
        	foreach (var personSkillPeriod in personSkillPeriods)
        	{
        		if (personSkillPeriod.Period.Contains(dateOnly))
        		{
        			affectedPersonSkills = personSkillPeriod.PersonSkillCollection;
					break;
        		}
        	}
            return affectedPersonSkills;
        }

        private static ICollection<IPersonSkill> filterPersonSkillByActivity(IEnumerable<IPersonSkill> personSkills, IActivity activity)
        {
        	var tempPersonSkills = new List<IPersonSkill>();
        	var tempSkills = new List<ISkill>();
        	foreach (var personSkill in personSkills.Where(personSkill => !tempSkills.Contains(personSkill.Skill)))
        	{
        		tempSkills.Add(personSkill.Skill);
        		tempPersonSkills.Add(personSkill);
        	}
			return tempPersonSkills.Where(personSkill => personSkill.Active).Where(personSkill => personSkill.Skill.Activity != null && personSkill.Skill.Activity.Equals(activity)).ToList();
        }

        public IEnumerable<ISkill> AffectedSkills
        {
            get
            {
				IList<ISkill> skills = new List<ISkill>();
				foreach (var affectedSkill in _skillCollection)
				{

					if (affectedSkill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill || affectedSkill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
						continue;
						
					skills.Add(affectedSkill);
				}
				return skills;
            }
        }

		private class PersonSkillPeriod
		{
			public DateOnlyPeriod Period { get; set; }
			public IList<IPersonSkill> PersonSkillCollection { get; set; }
		}
    }
}
