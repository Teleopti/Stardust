using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PersonProviderTest
    {
        [Test]
        public void VerifyWhenCreatingWithPersonListGetBackSamePersonList()
        {
			var persons = new List<IPerson> { PersonFactory.CreatePerson("Alan"), PersonFactory.CreatePerson("Bill") };
			var target = new PersonProvider(persons);

            IList<IPerson> resultList = target.GetPersons();

            Assert.IsNotNull(target);
            Assert.AreSame(persons[0], resultList[0]);
            Assert.AreSame(persons[1], resultList[1]);
        }
		
        [Test]
        public void ShouldBeAbleToGetAndSetDoLoadByPerson()
        {
			var persons = new List<IPerson> { PersonFactory.CreatePerson("Alan") };
			var target = new PersonProvider(persons);

            Assert.IsTrue(target.DoLoadByPerson);
            target.DoLoadByPerson = false;
            Assert.IsFalse(target.DoLoadByPerson);
        }

    }
}