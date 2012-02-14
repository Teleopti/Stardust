using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
            DateOnly startDate = new DateOnly(2000, 1, 1);

            var businessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            var deletedSite = SiteFactory.CreateSimpleSite();
            deletedSite.SetDeleted();
            businessUnit.AddSite(deletedSite);
            _businessUnits.Add(businessUnit);
            
            //Create new period.
            IPersonPeriod pPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate, businessUnit.SiteCollection[0].TeamCollection[0]);

            //Create new person.
            IPerson person = PersonFactory.CreatePerson("F","L");
            person.AddPersonPeriod(pPeriod);
            _persons.Add(person);
            
            //Create new person.
            IPerson person1 = PersonFactory.CreatePerson("User1","User1");
            _persons.Add(person1);
            _target = new PersonGroupPage();
        }

        [Test]
        public void CheckCreateGroupPage()
        {
            IGroupPage gPage = _target.CreateGroupPage(_businessUnits,
                                                       new GroupPageOptions(_persons)
                                                           {
                                                               CurrentGroupPageName = "Test Group Page",
                                                               CurrentGroupPageNameKey = "TestGroupPageKey"
                                                           });

            Assert.AreEqual("Test Group Page",gPage.Description.Name);
            Assert.AreEqual(_businessUnits.First().Name, gPage.RootNodeName);
            Assert.AreEqual("TestGroupPageKey", gPage.DescriptionKey);
            Assert.AreEqual(3, gPage.RootGroupCollection.Count);
            //Assert.AreEqual(1, gPage.RootGroupCollection[0].PersonCollection.Count); //Users
        }
    }
}