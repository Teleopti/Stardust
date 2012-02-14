using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PersonComparerTest
    {
        private PersonComparer _target;
        private Name name1;
        private Name name2;
        private Name name3;
        private Name name4;
        private Name name5;

        private IPerson person1;
        private IPerson person2;
        private IPerson person3;
        private IPerson person4;
        private IPerson person5;

        private IList<IPerson> persons;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "Teleopti.Ccc.Domain.Common.PersonComparer.#ctor"), SetUp]
        public void Setup()
        {
            _target = new PersonComparer();
            name1 = new Name("Brigitta", "Svensson");
            name2 = new Name("Alma", "Svensson");
            name3 = new Name("Dalma", "Nordqist");
            name4 = new Name("Cecilia", "Nordqist");
            name5 = new Name();

            person1 = PersonFactory.CreatePerson(name1);
            person2 = PersonFactory.CreatePerson(name2);
            person3 = PersonFactory.CreatePerson(name3);
            person4 = PersonFactory.CreatePerson(name4);
            person5 = PersonFactory.CreatePerson(name5);

            persons = new List<IPerson> { person1, person2, person3, person4, person5 };
        }

        [Test]
        public void VerifyCompare()
        {
            IList<IPerson> sortedList = persons.OrderBy(a => a, _target).ToList();
            Assert.AreEqual(sortedList.Count(), persons.Count);
            Assert.AreEqual(sortedList[0], person5);
            Assert.AreEqual(sortedList[1], person4);
            Assert.AreEqual(sortedList[2], person3);
            Assert.AreEqual(sortedList[3], person2);
            Assert.AreEqual(sortedList[4], person1);
        }
    }
}

