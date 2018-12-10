using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class ContactPersonViewModelTest
    {
        private IPerson _person;
        private ISkill _skill1;
        private ISkill _skill2;
        private ContactPersonViewModel _target;
        private ISite _site1;
        private ISite _site2;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("Barack", "Obama");
            _skill1 = SkillFactory.CreateSkill("Skill1");
            _skill2 = SkillFactory.CreateSkill("Skill2");
            _site1 = SiteFactory.CreateSimpleSite("Site1");
            _site2 = SiteFactory.CreateSimpleSite("Site2");
            _person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2005, 1, 1), _skill1));
            _person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(DateOnly.Today, _skill1, _skill2));
            _person.SetEmploymentNumber("001234");
            _person.Email = "barack.obama@whitehouse.gov";
            _person.PersonPeriodCollection[0].Team.SetDescription(new Description("Team1"));
            _person.PersonPeriodCollection[1].Team.SetDescription(new Description("Team2"));
            _person.PersonPeriodCollection[0].Team.Site = _site1;
            _person.PersonPeriodCollection[1].Team.Site = _site2;

            _target = new ContactPersonViewModel(_person);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_person, _target.ContainedEntity);
            Assert.AreEqual(_person.Name.FirstName,_target.FirstName);
            Assert.AreEqual(_person.Name.LastName, _target.LastName);
            Assert.AreEqual("Barack Obama",_target.FullName);
            Assert.AreEqual(_person.PersonPeriodCollection[1].Team,_target.TeamBelong);
            Assert.AreEqual(_person.PersonPeriodCollection[1].Team.Site, _target.SiteBelong);
            Assert.AreEqual(_person.Period(DateOnly.Today), _target.CurrentPeriod);
            Assert.AreEqual(_target.Email, _target.Email);
            Assert.AreEqual("Skill1; Skill2", _target.Skills);
        }

        [Test]
        public void VerifyFullNameWithOtherFormat()
        {
            _target = new ContactPersonViewModel(_person,new CommonNameDescriptionSetting("{LastName}, {FirstName}"));
            Assert.AreEqual("Obama, Barack", _target.FullName);
        }

        [Test]
        public void VerifyPropertiesWithOtherDate()
        {
            _target.CurrentDate = new DateOnly(2005, 1, 2);
            Assert.AreEqual(_person, _target.ContainedEntity);
            Assert.AreEqual(_person.Name.FirstName, _target.FirstName);
            Assert.AreEqual(_person.Name.LastName, _target.LastName);
            Assert.AreEqual(_person.PersonPeriodCollection[0].Team, _target.TeamBelong);
            Assert.AreEqual(_person.PersonPeriodCollection[0].Team.Site, _target.SiteBelong);
            Assert.AreEqual(_person.Period(new DateOnly(2005,1,2)), _target.CurrentPeriod);
            Assert.AreEqual(_person.Email, _target.Email);
            Assert.AreEqual(_person.EmploymentNumber,_target.EmploymentNumber);
            Assert.AreEqual("Skill1", _target.Skills);
        }

        [Test]
        public void VerifyPropertiesWithNoPersonPeriod()
        {
            _target.CurrentDate = new DateOnly(2004,12,31);
            Assert.AreEqual(_person, _target.ContainedEntity);
            Assert.AreEqual(_person.Name.FirstName, _target.FirstName);
            Assert.AreEqual(_person.Name.LastName, _target.LastName);
            Assert.IsNull(_target.TeamBelong);
            Assert.IsNull(_target.SiteBelong);
            Assert.IsNull(_target.CurrentPeriod);
            Assert.AreEqual(_person.Email, _target.Email);
            Assert.AreEqual(_person.EmploymentNumber,_target.EmploymentNumber);
            Assert.IsTrue(string.IsNullOrEmpty(_target.Skills));
        }

        [Test]
        public void VerifyCreateListOfViewModels()
        {
            var theList = ContactPersonViewModel.Parse(new List<IPerson> {_person},new CommonNameDescriptionSetting());
            Assert.AreEqual(1,theList.Count());
            Assert.AreEqual(_person,theList.First().ContainedEntity);
        }

        [Test]
        public void VerifyCanFilter()
        {
            Assert.IsTrue(_target.FilterByValue(string.Empty));
            Assert.IsTrue(_target.FilterByValue("ob"));
            Assert.IsFalse(_target.FilterByValue("bush"));
            Assert.IsTrue(_target.FilterByValue("barack."));
            Assert.IsFalse(_target.FilterByValue(".obamason"));
            Assert.IsTrue(_target.FilterByValue("kill2"));
            Assert.IsFalse(_target.FilterByValue("kill3"));
            Assert.IsTrue(_target.FilterByValue("team2"));
            Assert.IsTrue(_target.FilterByValue("eam2"));
            Assert.IsTrue(_target.FilterByValue("site2"));
            Assert.IsTrue(_target.FilterByValue("ite2"));
            Assert.IsTrue(_target.FilterByValue("001"));
            Assert.IsFalse(_target.FilterByValue("1123"));

            _target.CurrentDate = new DateOnly(2004, 12, 31);
            Assert.IsTrue(_target.FilterByValue("ob"));
            Assert.IsFalse(_target.FilterByValue("bush"));
            Assert.IsTrue(_target.FilterByValue("barack."));
            Assert.IsFalse(_target.FilterByValue(".obamass"));
            Assert.IsFalse(_target.FilterByValue("kill2"));
            Assert.IsFalse(_target.FilterByValue("kill3"));
            Assert.IsFalse(_target.FilterByValue("team2"));
            Assert.IsFalse(_target.FilterByValue("eam2"));
            Assert.IsFalse(_target.FilterByValue("site2"));
            Assert.IsFalse(_target.FilterByValue("ite2"));
            Assert.IsTrue(_target.FilterByValue("001"));
            Assert.IsFalse(_target.FilterByValue("1123"));
        }
    }
}

