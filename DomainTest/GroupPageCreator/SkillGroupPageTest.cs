using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
    [TestFixture]
    public class SkillGroupPageTest
    {
        private IGroupPageCreator<ISkill> _target;
        private IList<ISkill> _skills;
        private IGroupPageOptions _groupPageOptions;
 
        private ISkill _skill1;
        private ISkill _skill2;
       private IList<IPerson> _persons;
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
        private IPerson _person4;
        private string _rootName;

        [SetUp]
        public void Setup()
        {
            _target = new SkillGroupPage();

            _skill1 = SkillFactory.CreateSkill("skill1");
            _skill1.SetId(Guid.NewGuid());
            _skill2 = SkillFactory.CreateSkill("skill2");
            _skill2.SetId(Guid.NewGuid());

            _person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 01, 01), new List<ISkill> {_skill1});
            _person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 01, 01), new List<ISkill> { _skill1, _skill2 });
            _person3 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 01, 01), new List<ISkill>());
            _person4 = PersonFactory.CreatePerson();
        
            _persons = new List<IPerson>{_person1, _person2, _person3, _person4};
            _skills = new List<ISkill> { _skill2, _skill1 };

            _groupPageOptions = new GroupPageOptions(_persons);
            _rootName = "Skills";
            _groupPageOptions.CurrentGroupPageName = _rootName;

        }


        [Test]
        public void VerifyRootName()
        {
           IGroupPage groupPage = _target.CreateGroupPage(_skills, _groupPageOptions);
            Assert.AreEqual(groupPage.RootNodeName, "Skills");
        }

        [Test]
        public void VerifySkillsAsChildInRoot()
        {
            IGroupPage groupPage = _target.CreateGroupPage(_skills, _groupPageOptions);
            Assert.AreEqual(2, groupPage.RootGroupCollection.Count);
        }

		[Test]
		public void ShouldIgnoreInactiveSkills()
		{
			_person2.DeactivateSkill(_skill2,_person2.PersonPeriodCollection[0]);

			IGroupPage groupPage = _target.CreateGroupPage(_skills, _groupPageOptions);

			Assert.AreEqual(1, groupPage.RootGroupCollection.Count);
			Assert.AreEqual(2, groupPage.RootGroupCollection[0].PersonCollection.Count);
			Assert.IsTrue(groupPage.RootGroupCollection[0].PersonCollection.Contains(_person1));
			Assert.IsTrue(groupPage.RootGroupCollection[0].PersonCollection.Contains(_person2));
		}

        [Test]
        public void VerifySkillsAreInRightOrder()
        {
            IGroupPage groupPage = _target.CreateGroupPage(_skills, _groupPageOptions);
            Assert.AreEqual("skill1", groupPage.RootGroupCollection[0].Name);
            Assert.AreEqual("skill2", groupPage.RootGroupCollection[1].Name);
        }

        [Test]
        public void VerifyPersonsAreInRightSkillsAndPersonWithNoPersonPeriodIsNotInList()
        {
            IGroupPage groupPage = _target.CreateGroupPage(_skills, _groupPageOptions);
            Assert.AreEqual(2, groupPage.RootGroupCollection[0].PersonCollection.Count);
            Assert.IsTrue(groupPage.RootGroupCollection[0].PersonCollection.Contains(_person1));
            Assert.IsTrue(groupPage.RootGroupCollection[0].PersonCollection.Contains(_person2));
            Assert.AreEqual(1, groupPage.RootGroupCollection[1].PersonCollection.Count);
            Assert.IsTrue(groupPage.RootGroupCollection[1].PersonCollection.Contains(_person2));
        }

        [Test]
        public void VerifyPersonWithNoSkillsIsInRoot()
        {
            IGroupPage groupPage = _target.CreateGroupPage(_skills, _groupPageOptions);
            Assert.AreEqual(groupPage.RootNodeName, "Skills");
        }

        [Test]
        public void ShouldHaveSetIdForGroupPageGroupFromSkillEntityIfNotUserDefinedGroupPage()
        {
        	_groupPageOptions.CurrentGroupPageNameKey = "descriptionKey";
			IGroupPage gPage = _target.CreateGroupPage(_skills, _groupPageOptions);

            Assert.IsTrue(_skill1.Id.HasValue);
            Assert.IsTrue(_skill2.Id.HasValue);
            Assert.AreEqual(_skill1.Id, gPage.RootGroupCollection[0].Id);
            Assert.AreEqual(_skill2.Id, gPage.RootGroupCollection[1].Id);
        }

		[Test]
		public void ShouldNotHaveSetIdForGroupPageGroupFromSkillEntityIfUserDefinedGroupPage()
		{
			_groupPageOptions.CurrentGroupPageNameKey = null;
			IGroupPage gPage = _target.CreateGroupPage(_skills, _groupPageOptions);

			Assert.IsTrue(_skill1.Id.HasValue);
			Assert.IsTrue(_skill2.Id.HasValue);
			Assert.IsNull(gPage.RootGroupCollection[0].Id);
			Assert.IsNull(gPage.RootGroupCollection[1].Id);
		}
    }
}
