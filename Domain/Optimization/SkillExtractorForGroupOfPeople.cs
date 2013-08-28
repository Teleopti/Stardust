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
                var periodToday =  person.Period(DateOnly.Today);
                if(periodToday == null) continue;

                foreach (IPersonSkill personSkill in periodToday.PersonSkillCollection)
                    extractedSkills.Add(personSkill.Skill);
            }
            return extractedSkills.ToList();
        }
    }
}