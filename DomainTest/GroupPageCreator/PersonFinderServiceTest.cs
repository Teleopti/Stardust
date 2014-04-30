using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
    [TestFixture]
    public class PersonFinderServiceTest
    {
        private IList<IPerson> persons;
        private IPerson findPerson1;
        private IPerson findPerson2;
        private IPerson findPerson3;
        private PersonFinderService target;
        private IPerson findPerson4;
        private IPerson findPerson5;

        [SetUp]
        public void Setup()
        {
            persons = new List<IPerson>();
            var applicationFunction =  ApplicationFunctionFactory.CreateApplicationFunction("dontknow?");
            target = new PersonFinderService(new PersonIndexBuilder(applicationFunction, persons, new DateOnlyPeriod(2010, 1, 1, 2011, 1, 1)));
        }

        private void SetupPersons()
        {
            var personAccountUpdater = new MockRepository().StrictMock<IPersonAccountUpdater>();
            findPerson1 = PersonFactory.CreatePersonWithIdentityPermissionInfo("tommy");
            findPerson2 = PersonFactory.CreatePersonWithIdentityPermissionInfo("jonny");
            findPerson3 = PersonFactory.CreatePersonWithIdentityPermissionInfo("conny");
            findPerson4 = PersonFactory.CreatePersonWithIdentityPermissionInfo("ronny");
            findPerson5 = PersonFactory.CreatePersonWithIdentityPermissionInfo("benny");
            findPerson4.TerminatePerson(new DateOnly(2009,12,30), personAccountUpdater);
           
            findPerson1.AddPersonPeriod(new PersonPeriod(new DateOnly(new DateTime(2010,12,23)),new PersonContract(new Contract("Contract1"),new PartTimePercentage("PartTime3"),new ContractSchedule("CS3")), new Team {Description = new Description("Team1")} ));
            findPerson2.AddPersonPeriod(new PersonPeriod(new DateOnly(new DateTime(2010,12,23)),new PersonContract(new Contract("Contract2"),new PartTimePercentage("PartTime3"),new ContractSchedule("CS")), new Team {Description = new Description("Team1")} ));
            findPerson3.AddPersonPeriod(new PersonPeriod(new DateOnly(new DateTime(2010,12,23)),new PersonContract(new Contract("Contract3"),new PartTimePercentage("PartTime3"),new ContractSchedule("CS3")), new Team {Description = new Description("Team1")} ));
            findPerson4.AddPersonPeriod(new PersonPeriod(new DateOnly(new DateTime(2009,12,23)),new PersonContract(new Contract("Contract3"),new PartTimePercentage("PartTime3"),new ContractSchedule("CS3")), new Team {Description = new Description("Team1")} ));

            findPerson5.AddPersonPeriod(new PersonPeriod(new DateOnly(new DateTime(2007, 12, 23)), new PersonContract(new Contract("Contract3"), new PartTimePercentage("PartTime3"), new ContractSchedule("CS3")), new Team { Description = new Description("Team1") }));
            findPerson5.AddPersonPeriod(new PersonPeriod(new DateOnly(new DateTime(2009, 12, 23)), new PersonContract(new Contract("Contract3"), new PartTimePercentage("PartTime3"), new ContractSchedule("CS3")), new Team { Description = new Description("Team45") }));

            persons.Add(findPerson1);
            persons.Add(findPerson2);
            persons.Add(findPerson3);
            persons.Add(findPerson4);
            persons.Add(findPerson5);
        }

        [Test]
        public void ShouldFindPersons()
        {
            SetupPersons();
            var result = target.Find("");
            result.Should().Have.Count.EqualTo(0);

            result = target.Find("Team1 Contract3 cs3");

            result.Count.Should().Be.EqualTo(1);
            result.Should().Contain(findPerson3);
        }

        [Test]
        public void ShouldNotFindPerson()
        {
            //Exposure of a bug 
            var applicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("dontknow?");
            var per = PersonFactory.CreatePersonWithIdentityPermissionInfo("tommy");
            per.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                    {ApplicationLogOnName = "pa", Password = "pass"};
            per.AuthenticationInfo.Identity = "a";
            var isolatedtarget = new PersonFinderService(new PersonIndexBuilder(applicationFunction, new List<IPerson>{per}, new DateOnlyPeriod(2010, 1, 1, 2011, 1, 1)));
            var result = isolatedtarget.Find("apa");
            result.Count.Should().Be.EqualTo(0);
        }

        [Test]
        public void ShouldBeAbleToRebuildIndex()
        {
            var snubbe = new Person() {Name = new Name("Rågge", "Bågge")};
            target.Find("Rågge").Should().Be.Empty();
            persons.Add(snubbe);
            target.Find("Rågge").Should().Be.Empty();
            target.RebuildIndex();
            target.Find("Rågge").Should().Not.Be.Empty();
        }
    }
}
