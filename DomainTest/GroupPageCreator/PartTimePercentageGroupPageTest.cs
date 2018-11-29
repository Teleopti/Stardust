using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
    /// <summary>
    /// Tests PartTimePercentageGroupPage
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/7/2008
    /// </remarks>
    [TestFixture]
    public class PartTimePercentageGroupPageTest
    {
        private IGroupPage _target;
        private IList<IPartTimePercentage> _partTimePercentageCollection;
        private ICollection<IPerson> _personCollection;
        private PartTimePercentageGroupPage _obj;
        private IGroupPage _groupPage;
        private string _groupPageName;
        private string _groupPageNameKey;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/7/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            // setup PersonPeriod
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2008, 07, 07));
            IPersonContract personContract = PersonContractFactory.CreatePersonContract();
            IPartTimePercentage partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("Test");
            partTimePercentage.SetId(Guid.NewGuid());

            personContract.PartTimePercentage = partTimePercentage;
            personPeriod.PersonContract = personContract;

            // setup Person
            IPerson person = PersonFactory.CreatePerson("Name");
            person.AddPersonPeriod(personPeriod);

            // setup RootPersonGroup
            IRootPersonGroup rootPersonGroup = new RootPersonGroup("Name");
            rootPersonGroup.AddPerson(person);

            // Initializes _target
            _target = new GroupPage("Name");
            _target.AddRootPersonGroup(rootPersonGroup);

            // Initializes _partTimePercentageCollection
            _partTimePercentageCollection = new List<IPartTimePercentage> {partTimePercentage};

            // Initializes _personCollection
            _personCollection = new List<IPerson> {person};

            _obj = new PartTimePercentageGroupPage();

            _groupPageName = "CurrentGroupPageName";
            _groupPageNameKey = "CurrentGroupPageNameKey";
            _groupPage = _obj.CreateGroupPage(_partTimePercentageCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });
        }

        /// <summary>
        /// Verifies the root person group count.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/7/2008
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
            ((IDeleteTag)_partTimePercentageCollection[0]).SetDeleted();
            _target = _obj.CreateGroupPage(_partTimePercentageCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });
            Assert.AreEqual(0, _target.RootGroupCollection.Count);
        }

        /// <summary>
        /// Verifies the person count.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/7/2008
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
        public void ShouldHaveSetIdForGroupPageGroupFromPartTimePercentageEntityIfNotUserDefinedGroupPage()
        {
            Assert.IsTrue(_partTimePercentageCollection[0].Id.HasValue);
            Assert.AreEqual(_partTimePercentageCollection[0].Id, _groupPage.RootGroupCollection[0].Id);
        }

		[Test]
		public void ShouldNotHaveSetIdForGroupPageGroupFromPartTimePercentageEntityIfUserDefinedGroupPage()
		{
			_groupPageNameKey = null;
			_groupPage = _obj.CreateGroupPage(_partTimePercentageCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });

			Assert.IsTrue(_partTimePercentageCollection[0].Id.HasValue);
			Assert.IsNull(_groupPage.RootGroupCollection[0].Id);
		}
    }
}