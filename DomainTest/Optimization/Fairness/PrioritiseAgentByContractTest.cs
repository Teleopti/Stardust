//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;
//using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
//using Teleopti.Ccc.TestCommon.FakeData;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.DomainTest.Optimization.Fairness
//{
//    [TestFixture]
//    public class PrioritiseAgentByContractTest
//    {

//        private IPrioritiseAgentByContract _target; 

//        [SetUp]
//        public void Setup()
//        {
//            _target = new PrioritiseAgentByContract();
//        }

//        [Test]
//        public void TestPriorityByName()
//        {
//            var personList = new List<IPerson>();
//            personList.Add(PersonFactory.CreatePerson("perzi"));
//            personList.Add(PersonFactory.CreatePerson("zynx"));
//            personList.Add(PersonFactory.CreatePerson("asad"));
//            personList.Add(PersonFactory.CreatePerson("mirza"));

//            var result = _target.GetPriortiseAgentByName(personList);

//            Assert.AreEqual(result[0].Name.FirstName, "asad");
//            Assert.AreEqual(result[1].Name.FirstName, "mirza");
//            Assert.AreEqual(result[2].Name.FirstName, "perzi");
//            Assert.AreEqual(result[3].Name.FirstName, "zynx");
//        }

//        [Test]
//        public void TestPriorityByStartDate()
//        {
//            var personList = new List<IPerson>();
//            var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2013, 11, 12));
//            var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2013, 10, 12));
//            var person3 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2013, 12, 12));
//            var person4 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2013, 9, 10));

//            personList.Add(person1 );
//            personList.Add(person2 );
//            personList.Add(person3 );
//            personList.Add(person4 );

//            var result = _target.GetPriortiseAgentByStartDate(personList);

//            Assert.AreEqual(result[0], person4 );
//            Assert.AreEqual(result[1], person2 );
//            Assert.AreEqual(result[2], person1 );
//            Assert.AreEqual(result[3], person3 );
//        }
//    }
//}
