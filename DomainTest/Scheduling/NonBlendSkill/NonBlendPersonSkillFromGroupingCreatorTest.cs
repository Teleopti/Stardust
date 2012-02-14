using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.NonBlendSkill
{
    [TestFixture]
    public class NonBlendPersonSkillFromGroupingCreatorTest
    {
        private MockRepository _mocks;
        private INonBlendPersonSkillFromGroupingCreator _nonBlendPersonSkillFromGroupingCreator;
        private IPerson _person1;
        private IPerson _person2;
        private Skill _skill;
        private List<IPerson> _persons;
        private IPersonPeriod _personPeriod2;
        private IPersonPeriod _personPeriod1;
        private DateOnly _dateOnly;
        private List<IPersonSkill> _personSkills;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _nonBlendPersonSkillFromGroupingCreator = new NonBlendPersonSkillFromGroupingCreator();
            _skill = new Skill("nonBlend", "", Color.Firebrick, 15,
                                  new SkillTypePhone(new Description(), ForecastSource.NonBlendSkill));
            _person1 = _mocks.StrictMock<IPerson>();
            _person2 = _mocks.StrictMock<IPerson>();
            _persons = new List<IPerson> {_person1, _person2};
            _personPeriod1 = _mocks.StrictMock<IPersonPeriod>();
            _personPeriod2 = _mocks.StrictMock<IPersonPeriod>();
            _dateOnly = new DateOnly(2011, 1, 14);

            _personSkills = _mocks.StrictMock<List<IPersonSkill>>();
        }

        [Test]
        public void ShouldRemoveAllNonBlendSkillsOnPersonFirst()
        {
            Expect.Call(_person1.Period(_dateOnly)).Return(_personPeriod1);
            Expect.Call(_person2.Period(_dateOnly)).Return(_personPeriod2);
            Expect.Call(_personPeriod1.PersonNonBlendSkillCollection).Return(_personSkills);
            Expect.Call(_personPeriod2.PersonNonBlendSkillCollection).Return(_personSkills);
            Expect.Call(() => _personSkills.Clear());
            Expect.Call(() => _personPeriod1.AddPersonNonBlendSkill(null)).IgnoreArguments();
            Expect.Call(() => _personPeriod2.AddPersonNonBlendSkill(null)).IgnoreArguments();
            _mocks.ReplayAll();
            _nonBlendPersonSkillFromGroupingCreator.ProcessPersons(_skill, _persons, _dateOnly);
            _mocks.VerifyAll();
        }
        [Test]
        public void ShouldAddPersonSkillToAllPersons()
        {
            var personSkills1 = new List<IPersonSkill>();
            var personSkills2 = new List<IPersonSkill>();
            Expect.Call(_person1.Period(_dateOnly)).Return(_personPeriod1);
            Expect.Call(_person2.Period(_dateOnly)).Return(_personPeriod2);
            Expect.Call(_personPeriod1.PersonNonBlendSkillCollection).Return(personSkills1);
            Expect.Call(_personPeriod2.PersonNonBlendSkillCollection).Return(personSkills2);

            Expect.Call(() =>_personPeriod1.AddPersonNonBlendSkill(null)).IgnoreArguments();
            Expect.Call(() =>_personPeriod2.AddPersonNonBlendSkill(null)).IgnoreArguments();
            _mocks.ReplayAll();
            _nonBlendPersonSkillFromGroupingCreator.ProcessPersons(_skill,_persons,_dateOnly);
            _mocks.VerifyAll();
        }
    }

    

}