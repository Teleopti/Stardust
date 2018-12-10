using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
    [TestFixture]
    public class ContractGroupPageTest
    {
        private ContractGroupPage _target;
        private IList<IContract> _contracts;
        private ICollection<IPerson> _persons;
        private IGroupPage _groupPage;

        [SetUp]
        public void TestInit()
        {
            _contracts = new List<IContract>();
            _persons = new Collection<IPerson>();
            _groupPage = new GroupPage("Test Group Page");
            for (int i = 0; i < 5; i++)
            {
                //Create datetime
                DateOnly startDate = new DateOnly(2000, 1, 1);
                IPersonContract personContract = PersonContractFactory.CreatePersonContract();

                //Create Contract
                IContract contract = ContractFactory.CreateContract("MyContract" + i.ToString(CultureInfo.CurrentCulture));
                contract.SetId(Guid.NewGuid());
                personContract.Contract = contract;
                _contracts.Add(personContract.Contract);

                //Create rootPersonGroup & Add into GroupPage.
                IRootPersonGroup rGroup = new RootPersonGroup(personContract.Contract.Description.ToString());

                //Create team
                ITeam team = TeamFactory.CreateSimpleTeam("1stTeam" + i.ToString(CultureInfo.CurrentCulture));

                //Create new period.
                IPersonPeriod pPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate, personContract, team);

                //Create new person.
                IPerson person = PersonFactory.CreatePerson("F" + i.ToString(CultureInfo.CurrentCulture), "L" + i.ToString(CultureInfo.CurrentCulture));
                person.AddPersonPeriod(pPeriod);
                _persons.Add(person);
                rGroup.AddPerson(person);

                //Create new person.
                IPerson person1 = PersonFactory.CreatePerson("Ff" + i.ToString(CultureInfo.CurrentCulture), "Ll" + i.ToString(CultureInfo.CurrentCulture));
                person1.AddPersonPeriod(pPeriod);
                _persons.Add(person1);
                rGroup.AddPerson(person1);

                _groupPage.AddRootPersonGroup(rGroup);
            }
            _target = new ContractGroupPage();
        }

        [Test]
        public void CheckCreateGroupPage()
        {
            IGroupPage gPage = _target.CreateGroupPage(_contracts,
                                                       new GroupPageOptions(_persons)
                                                           {
                                                               CurrentGroupPageName = "Test Group Page",
                                                               CurrentGroupPageNameKey = "TestGroupPageKey"
                                                           });

            Assert.AreEqual("Test Group Page", gPage.Description.Name);
            Assert.AreEqual("TestGroupPageKey", gPage.DescriptionKey);
            Assert.AreEqual(gPage.RootGroupCollection.Count, _groupPage.RootGroupCollection.Count);
            Assert.AreEqual(2, gPage.RootGroupCollection[0].PersonCollection.Count);
        }

        [Test]
        public void ShouldExcludeDeletedContract()
        {
            ((IDeleteTag)_contracts[0]).SetDeleted();
            IGroupPage gPage = _target.CreateGroupPage(_contracts,
                                                       new GroupPageOptions(_persons)
                                                       {
                                                           CurrentGroupPageName = "Test Group Page",
                                                           CurrentGroupPageNameKey = "TestGroupPageKey"
                                                       });
            Assert.AreEqual(4,gPage.RootGroupCollection.Count);
        }

        [Test]
        public void ShouldHaveSetIdForGroupPageGroupFromContractEntityIfNotUserDefinedGroupPage()
        {
            IGroupPage gPage = _target.CreateGroupPage(_contracts,
                                                       new GroupPageOptions(_persons)
                                                       {
                                                           CurrentGroupPageName = "Test Group Page",
                                                           CurrentGroupPageNameKey = "TestGroupPageKey"
                                                       });

            Assert.IsTrue(_contracts[0].Id.HasValue);
            Assert.AreEqual(_contracts[0].Id, gPage.RootGroupCollection[0].Id);
        }

		[Test]
		public void ShouldNotHaveSetIdForGroupPageGroupFromContractEntityIfUserDefinedGroupPage()
		{
			IGroupPage gPage = _target.CreateGroupPage(_contracts,
													   new GroupPageOptions(_persons)
													   {
														   CurrentGroupPageName = "Test Group Page",
														   CurrentGroupPageNameKey = null
													   });

			Assert.IsTrue(_contracts[0].Id.HasValue);
			Assert.IsNull(gPage.RootGroupCollection[0].Id);
		}
    }
}