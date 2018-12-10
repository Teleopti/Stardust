using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			var dateOnly = DateOnly.Today;
			return _persons.Select(person => person.Period(dateOnly)).Where(pp => pp != null)
				.SelectMany(ps => ps.PersonSkillCollection).Select(s => s.Skill).Distinct().ToArray();
        }
    }
}