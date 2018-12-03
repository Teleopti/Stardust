using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class PersonSkillModelTest
    {
        private PersonSkillModel _target;
        private IPersonSkill _personSkill;

        [SetUp]
        public void TestInit()
        {
            _target = new PersonSkillModel();

            _personSkill = PersonSkillFactory.CreatePersonSkill("Skill1", 1);
	        var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), Enumerable.Empty<ISkill>());
	        person.AddSkill(_personSkill, person.PersonPeriodCollection[0]);
            _target.ContainedEntity = _personSkill;
        }

        [Test]
        public void VerifyCurrentProperties()
        {
            Assert.IsNotNull(_target.PersonSkill);
            Assert.IsNotNull(_target.DescriptionText);
        }

        [Test]
        public void VerifyCanGetPersonSkill()
        {
            Assert.AreEqual(_personSkill, _target.PersonSkill);
        }

        [Test]
        public void VerifyCanGetAndSetTriState()
        {
            _target.TriState = 2;
            Assert.AreEqual(2, _target.TriState);
        }

        [Test]
        public void VerifyCanGetAndSetPersonSkillExistsInPersonCount()
        {
            _target.PersonSkillExistsInPersonCount = 2;
            Assert.AreEqual(2, _target.PersonSkillExistsInPersonCount);
        }

        [Test]
        public void VerifyCanGetDescription()
        {
            Assert.AreEqual(_personSkill.Skill.Name, _target.DescriptionText);
        }

		[Test]
		public void ShouldNotSaveLessThanOneOrMoreThan999()
		{
			_target.Proficiency = 10;
			Assert.That(_target.Proficiency, Is.EqualTo(10));
			_target.Proficiency = 1000;
			Assert.That(_target.Proficiency, Is.EqualTo(10));
			_target.Proficiency = 0;
			Assert.That(_target.Proficiency, Is.EqualTo(10));
		}

		[Test]
		public void ProficiencyValuesShouldReturnAListWithOneAsDefault()
		{
			Assert.That(_target.ProficiencyValues.Count,Is.EqualTo(1));
			_target.ProficiencyValues = new StringCollection();
			Assert.That(_target.ProficiencyValues.Count, Is.EqualTo(1));
		}

		[Test]
		public void ProficiencyValuesShouldAddSeveralValuesIfMoreThanOne()
		{
			_target.AddProficiencyValue("100");
			_target.AddProficiencyValue("50");
			Assert.That(_target.ProficiencyValues.Count, Is.EqualTo(3));
			Assert.That(_target.ProficiencyValues[0],Is.EqualTo(Resources.SeveralValues));
		}
    }
}
