using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SkillExtractorForGroupOfPeopleTest
    {
        private ISkillExtractor _target;


        [Test]
        public void ShouldReturnTheUnionOfAllSkills()
        {
            ISkill skill1 = SkillFactory.CreateSkill("Skill1");
            ISkill skill2 = SkillFactory.CreateSkill("Skill2");
            ISkill skill3 = SkillFactory.CreateSkill("Skill3");

            IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(),
                                                                         new List<ISkill> {skill1, skill2});
            IPerson person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(),
                                                             new List<ISkill> { skill1, skill3 });

            IList<IPerson> persons = new List<IPerson> {person1, person2};
            _target = new SkillExtractorForGroupOfPeople(persons);
            IEnumerable<ISkill> result = _target.ExtractSkills();

            Assert.AreEqual(3, result.Count());

        }

        [Test]
        public void ShouldOnlyHandleSkillsWithinGivenPeriod()
        {
            ISkill skill1 = SkillFactory.CreateSkill("Skill1");
            ISkill skill2 = SkillFactory.CreateSkill("Skill2");

            IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(),
                                                             new List<ISkill> { skill1, skill2 });
            IPersonPeriod period2 = PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2000, 01, 01), skill2);

            person1.AddPersonPeriod(period2);

            IList<IPerson> persons = new List<IPerson> { person1 };

            _target = new SkillExtractorForGroupOfPeople(persons);
            IEnumerable<ISkill> result = _target.ExtractSkills();
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Skill2", new List<ISkill>(result)[0].Name);
        }
    }
}