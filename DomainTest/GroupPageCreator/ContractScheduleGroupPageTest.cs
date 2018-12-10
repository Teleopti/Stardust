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
    /// Tests ContractScheduleGroupPage
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/8/2008
    /// </remarks>
    [TestFixture]
    public class ContractScheduleGroupPageTest
    {
        private IGroupPage _target;
        private IGroupPage _groupPage;
        private IList<IContractSchedule> _contractScheduleCollection;
        private ICollection<IPerson> _personCollection;
        private ContractScheduleGroupPage _obj;
        private string _groupPageName;
        private string _groupPageNameKey;

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
            // setup PersonPeriod
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2008, 07, 07));
            IPersonContract personContract = PersonContractFactory.CreatePersonContract();
            IContractSchedule contractSchedule = ContractScheduleFactory.CreateContractSchedule("Test");
            contractSchedule.SetId(Guid.NewGuid());

            personContract.ContractSchedule = contractSchedule;
            personPeriod.PersonContract = personContract;

            // setup Person
            IPerson person = PersonFactory.CreatePerson("Name");
            person.AddPersonPeriod(personPeriod);

            // setup RootPersonGroup
            IRootPersonGroup rootPersonGroup = new RootPersonGroup("Name");
            rootPersonGroup.AddPerson(person);

            // Initializes _groupPage
            _groupPage = new GroupPage("Name");
            _groupPage.AddRootPersonGroup(rootPersonGroup);

            _contractScheduleCollection = new List<IContractSchedule> {contractSchedule};

            _personCollection = new List<IPerson> {person};

            _obj = new ContractScheduleGroupPage();

            _groupPageName = "CurrentGroupPageName";
            _groupPageNameKey = "CurrentGroupPageNameKey";
            _target = _obj.CreateGroupPage(_contractScheduleCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });
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
            int expectedRootPageCount = _target.RootGroupCollection.Count;
            int actualRootPageCount = _groupPage.RootGroupCollection.Count;
            Assert.AreEqual(expectedRootPageCount, actualRootPageCount);
        }

        [Test]
        public void ShouldExcludeDeletedContractSchedule()
        {
            ((IDeleteTag)_contractScheduleCollection[0]).SetDeleted();
            _target = _obj.CreateGroupPage(_contractScheduleCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });
            Assert.AreEqual(0,_target.RootGroupCollection.Count);
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
            int expectedRootPageCount = _target.RootGroupCollection[0].PersonCollection.Count;
            int actualRootPageCount = _groupPage.RootGroupCollection[0].PersonCollection.Count;
            Assert.AreEqual(expectedRootPageCount, actualRootPageCount);
        }

        [Test]
        public void VerifyDescription()
        {
            Assert.AreEqual(_groupPageName, _target.Description.Name);
            Assert.AreEqual(_groupPageNameKey, _target.DescriptionKey);
        }

        [Test]
        public void ShouldHaveSetIdForGroupPageGroupFromContractScheduleEntityIfNotUserDefinedGroupPage()
        {
            Assert.IsTrue(_contractScheduleCollection[0].Id.HasValue);
            Assert.AreEqual(_contractScheduleCollection[0].Id, _target.RootGroupCollection[0].Id);
        }

		[Test]
		public void ShouldHaveNotSetIdForGroupPageGroupFromContractScheduleEntityIfUserDefinedGroupPage()
		{
			_groupPageNameKey = null;
			_target = _obj.CreateGroupPage(_contractScheduleCollection, new GroupPageOptions(_personCollection) { CurrentGroupPageName = _groupPageName, CurrentGroupPageNameKey = _groupPageNameKey });

			Assert.IsTrue(_contractScheduleCollection[0].Id.HasValue);
			Assert.IsNull(_target.RootGroupCollection[0].Id);
		}
    }
}