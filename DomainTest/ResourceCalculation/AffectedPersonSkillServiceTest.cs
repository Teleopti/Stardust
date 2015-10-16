using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class AffectedPersonSkillServiceTest
    {
        private IAffectedPersonSkillService _target;
        private DateOnlyPeriod _outerPeriod;
        private IList<ISkill> _validSkills;

        [SetUp]
        public void Setup()
        {
            _outerPeriod = new DateOnlyPeriod(2000,1,1,2000,1,31);
            _validSkills = new List<ISkill>();
            _target = new AffectedPersonSkillService(_outerPeriod, _validSkills);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Domain.ResourceCalculation.AffectedPersonSkillService"), Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void CannotUseNullAsValidSkills()
        {
            new AffectedPersonSkillService(_outerPeriod, null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void CannotExecuteOnDateOutsideOuterPeriod()
        {
            _target.Execute(new Person(), new Activity("f"), new DateOnly(1999, 12, 12));
        }

        [Test]
        public void CanGetValidSkills()
        {
            ISkill skill = SkillFactory.CreateSkill("d");
            _validSkills.Add(skill);
            IEnumerable<ISkill> valid = _target.AffectedSkills;
            Assert.AreEqual(1, valid.Count());
            Assert.AreSame(skill, valid.First());
        }

        [Test]
        public void VerifyOnlyGetValidSkills()
        {
            IActivity act = new Activity("sd");
            ISkill validSkill = SkillFactory.CreateSkill("valid");
            validSkill.Activity = act;
            ISkill nonValidSkill = SkillFactory.CreateSkill("non valid");
            nonValidSkill.Activity = act;
            _validSkills.Add(validSkill);
            IPerson person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1), new List<ISkill> {validSkill, nonValidSkill});

            ICollection<IPersonSkill> validPersonSkill = _target.Execute(person, act, new DateOnly(2000, 1, 1));
            Assert.AreEqual(1, validPersonSkill.Count);
            Assert.AreSame(validSkill, validPersonSkill.First().Skill);
        }

        [Test]
        public void VerifyOnlyGetValidActivities()
        {
            IActivity act = new Activity("sd");
            ISkill validSkill = SkillFactory.CreateSkill("valid");
            validSkill.Activity = act;
            _validSkills.Add(validSkill);
            IPerson person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1), new List<ISkill> { validSkill });

            ICollection<IPersonSkill> validPersonSkill = _target.Execute(person, new Activity("d"), new DateOnly(2000, 1, 1));
            Assert.AreEqual(0, validPersonSkill.Count);
        }

        [Test]
        public void VerifyOnlyCorrectPersonPeriod()
        {
            IActivity act = new Activity("sd");
            ISkill validSkill = SkillFactory.CreateSkill("valid");
            validSkill.Activity = act;
            _validSkills.Add(validSkill);
            IPerson person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2001, 1, 1), new List<ISkill> { validSkill });

            ICollection<IPersonSkill> validPersonSkill = _target.Execute(person, act, new DateOnly(2000, 1, 1));
            Assert.AreEqual(0, validPersonSkill.Count);
        }

        [Test]
        public void VerifyTwoPersonPeriodsInPeriod()
        {
            IActivity act = new Activity("sd");
            ISkill validSkill = SkillFactory.CreateSkill("valid");
            validSkill.Activity = act;
            _validSkills.Add(validSkill);
            ISkill validSkill2 = SkillFactory.CreateSkill("valid2");
            validSkill2.Activity = act;
            _validSkills.Add(validSkill2);
            IPerson person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1), new List<ISkill> { validSkill });
            IPersonPeriod pPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000,1,11));
			person.AddPersonPeriod(pPeriod2);
			person.AddSkill(new PersonSkill(validSkill2, new Percent(1)),pPeriod2);
            
            ICollection<IPersonSkill> validPersonSkill = _target.Execute(person, act, new DateOnly(2000, 1, 20));
            Assert.AreEqual(1, validPersonSkill.Count);
            Assert.AreSame(validSkill2, validPersonSkill.First().Skill);
            validPersonSkill = _target.Execute(person, act, new DateOnly(2000, 1, 2));
            Assert.AreEqual(1, validPersonSkill.Count);
            Assert.AreEqual(validSkill, validPersonSkill.First().Skill);
        }

        [Test]
        public void VerifyTwoActivitiesAffectedInPeriod()
        {
            IActivity act1 = new Activity("1");
            ISkill validSkill = SkillFactory.CreateSkill("valid");
            validSkill.Activity = act1;
            _validSkills.Add(validSkill);
            IActivity act2 = new Activity("2");
            ISkill validSkill2 = SkillFactory.CreateSkill("valid2");
            validSkill2.Activity = act2;
            _validSkills.Add(validSkill2);
            IPerson person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1), new List<ISkill> { validSkill, validSkill2 });

            ICollection<IPersonSkill> validPersonSkillFirst = _target.Execute(person, act1, new DateOnly(2000, 1, 20));
            Assert.AreEqual(1, validPersonSkillFirst.Count);
            Assert.AreSame(validSkill, validPersonSkillFirst.First().Skill);

            ICollection<IPersonSkill> validPersonSkillSecond = _target.Execute(person, act2, new DateOnly(2000, 1, 20));
            Assert.AreEqual(1, validPersonSkillSecond.Count);
            Assert.AreSame(validSkill2, validPersonSkillSecond.First().Skill);

        }

		[Test]
		public void AffectedSkillsShouldNotContainMaxSeatSkillOrNonBlendSkill()
		{
			_validSkills.Add(SkillFactory.CreateSkill("vanligt"));
			_validSkills.Add(SkillFactory.CreateSiteSkill("site"));
			_validSkills.Add(SkillFactory.CreateNonBlendSkill("non"));
			_target = new AffectedPersonSkillService(_outerPeriod, _validSkills);
			Assert.AreEqual(1, _target.AffectedSkills.Count());
			ISkill skill = _target.AffectedSkills.FirstOrDefault();
			Assert.AreEqual("vanligt", skill.Name);
		}
    }
}
