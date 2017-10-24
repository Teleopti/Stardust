using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PersonProviderTest
    {
        private PersonProvider _target;
        private IList<IPerson> _persons;
        private MockRepository _mockRepository;
        private IPersonRepository _personRepository;

        [SetUp]
        public void Setup()
        {
            _persons = new List<IPerson> {PersonFactory.CreatePerson("Alan"), PersonFactory.CreatePerson("Bill")};
            _mockRepository = new MockRepository();
            _personRepository = _mockRepository.StrictMock<IPersonRepository>();

        }

        [Test]
        public void VerifyWhenCreatingWithPersonListGetBackSamePersonList()
        {
            _target = new PersonProvider(_persons);

            IList<IPerson> resultList = _target.GetPersons();

            Assert.IsNotNull(_target);
            Assert.AreSame(_persons[0], resultList[0]);
            Assert.AreSame(_persons[1], resultList[1]);
        }


        [Test]
        public void ShouldBeAbleToGetAndSetDoLoadByPerson()
        {
            _target = new PersonProvider(_persons);

            Assert.IsFalse(_target.DoLoadByPerson);
            _target.DoLoadByPerson = true;
            Assert.IsTrue(_target.DoLoadByPerson);
        }

    }
}