using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class SkillExtractorForGroupOfPeople : ISkillExtractor
    {
        private readonly IEnumerable<IPerson> _persons;

        public SkillExtractorForGroupOfPeople(IEnumerable<IPerson> persons)
        {
            _persons = persons;
        }


        public IEnumerable<ISkill> ExtractSkills()
        {
            HashSet<ISkill> extractedSkills = new HashSet<ISkill>();

            foreach (IPerson person in _persons)
            { 
                IList<IPersonPeriod> periodsToday =  person.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
                if(periodsToday.Count == 0)
                    continue;
                IPersonPeriod currentPeriod = periodsToday[0];
                foreach (IPersonSkill personSkill in currentPeriod.PersonSkillCollection)
                    extractedSkills.Add(personSkill.Skill);
            }
            return extractedSkills.ToList();
        }
    }
}