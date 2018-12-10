using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
    [TestFixture]
    public class PersonGroupPageTest
    {
        private PersonGroupPage _target;
        private ICollection<IBusinessUnit> _businessUnits;
        private ICollection<IPerson> _persons;

        [SetUp]
        public void TestInit()
        {
            _businessUnits = new Collection<IBusinessUnit>();
            _persons = new Collection<IPerson>();

            //Create datetime
            var startDate = new DateOnly(2000, 1, 1);

            var businessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            var deletedSite = SiteFactory.CreateSimpleSite();
            deletedSite.SetDeleted();
            businessUnit.AddSite(deletedSite);
            _businessUnits.Add(businessUnit);
            
            //Create new period.
            var pPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate, businessUnit.SiteCollection[0].TeamCollection[0]);

            //Create new person.
            var person = PersonFactory.CreatePerson("F","L");
            person.AddPersonPeriod(pPeriod);
            _persons.Add(person);
            
            //Create new person.
            var person1 = PersonFactory.CreatePerson("User1","User1");
            _persons.Add(person1);
            _target = new PersonGroupPage();
        }

        [Test]
        public void CheckCreateGroupPage()
		{
			var option = new GroupPageOptions(_persons)
			{
				CurrentGroupPageName = "Test Group Page",
				CurrentGroupPageNameKey = "TestGroupPageKey"
			};

			var gPage = _target.CreateGroupPage(_businessUnits,option);

            Assert.AreEqual("Test Group Page",gPage.Description.Name);
            Assert.AreEqual(_businessUnits.First().Name, gPage.RootNodeName);
            Assert.AreEqual("TestGroupPageKey", gPage.DescriptionKey);
            Assert.AreEqual(3, gPage.RootGroupCollection.Count);
            //Assert.AreEqual(1, gPage.RootGroupCollection[0].PersonCollection.Count); //Users
        }
    }
}