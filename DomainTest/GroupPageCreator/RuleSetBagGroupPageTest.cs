using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
    /// <summary>
    /// Tests RuleSetBagGroupPage
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/8/2008
    /// </remarks>
    [TestFixture]
    public class RuleSetBagGroupPageTest
    {
        private IGroupPage _target;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private ICollection<IPerson> _personCollection;
        private RuleSetBagGroupPage _obj;
        private string _groupPageName;
        private string _groupPageNameKey;
        private IGroupPage _groupPage;
        private IPerson _person;
        private IPerson _personWithoutShiftBag;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/8/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            IRuleSetBag ruleSetBag = new RuleSetBag {Description = new Description("TestName")};
            ruleSetBag.SetId(Guid.NewGuid());
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2008, 07, 07));
            personPeriod.RuleSetBag = ruleSetBag;
            IPersonPeriod personPeriodWithoutShiftBag = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 10, 10));

            // setup Persons
            _person = PersonFactory.CreatePerson("Name");
            _person.AddPersonPeriod(personPeriod);

            _personWithoutShiftBag = PersonFactory.CreatePerson("I have no shiftbag!");
            _personWithoutShiftBag.AddPersonPeriod(personPeriodWithoutShiftBag);
            
            // setup RootPersonGroup
            IRootPersonGroup rootPersonGroup = new RootPersonGroup("Name");
            rootPersonGroup.AddPerson(_person);

            // Initializes _target
            _target = new GroupPage("Name");
            _target.AddRootPersonGroup(rootPersonGroup);

            // Initializes _ruleSetBagCollection
            _ruleSetBagCollection = new List<IRuleSetBag> {ruleSetBag};

            // Initializes _personCollection
            _personCollection = new List<IPerson> {_person, _personWithoutShiftBag};

            _obj = new RuleSetBagGroupPage();
            _groupPageName = "CurrentGroupPageName";
            _groupPageNameKey = "CurrentGroupPageNameKey";
            _groupPage = _obj.CreateGroupPage(_ruleSetBagCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });
        }

        /// <summary>
        /// Verifies the root person group count.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/8/2008
        /// </remarks>
        [Test]
        public void VerifyRootPersonGroupCount()
        {
            int expectedRootPageCount = _groupPage.RootGroupCollection.Count;
            int actualRootPageCount = _target.RootGroupCollection.Count;
            Assert.AreEqual(expectedRootPageCount, actualRootPageCount);
        }

        [Test]
        public void ShouldExcludeDeletedContractSchedule()
        {
            ((IDeleteTag)_ruleSetBagCollection[0]).SetDeleted();
            _target = _obj.CreateGroupPage(_ruleSetBagCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });
            Assert.AreEqual(0, _target.RootGroupCollection.Count);
        }

        /// <summary>
        /// Verifies the person count.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/8/2008
        /// </remarks>
        [Test]
        public void VerifyPersonCount()
        {
            int expectedRootPageCount = _groupPage.RootGroupCollection[0].PersonCollection.Count;
            int actualRootPageCount = _target.RootGroupCollection[0].PersonCollection.Count;
            Assert.AreEqual(expectedRootPageCount, actualRootPageCount);
        }

        [Test]
        public void VerifyDescription()
        {
            Assert.AreEqual(_groupPageName, _groupPage.Description.Name);
            Assert.AreEqual(_groupPageNameKey, _groupPage.DescriptionKey);
        }

        [Test]
        public void ShouldIncludeOnlyOneSpecificPerson()
        {
            Assert.IsTrue(_target.RootGroupCollection[0].PersonCollection.Contains(_person));
            Assert.IsFalse(_target.RootGroupCollection[0].PersonCollection.Contains(_personWithoutShiftBag));
        }

        [Test]
        public void ShouldHaveSetIdForGroupPageGroupFromRuleSetBagEntityIfNotUserDefinedGroupPage()
        {
            Assert.IsTrue(_ruleSetBagCollection[0].Id.HasValue);
            Assert.AreEqual(_ruleSetBagCollection[0].Id, _groupPage.RootGroupCollection[0].Id);
        }

		[Test]
		public void ShouldNotHaveSetIdForGroupPageGroupFromRuleSetBagEntityIfUserDefinedGroupPage()
		{
			_groupPageNameKey = null;
			_groupPage = _obj.CreateGroupPage(_ruleSetBagCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });

			Assert.IsTrue(_ruleSetBagCollection[0].Id.HasValue);
			Assert.IsNull(_groupPage.RootGroupCollection[0].Id);
		}
    }
}