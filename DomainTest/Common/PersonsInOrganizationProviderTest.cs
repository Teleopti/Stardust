using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PersonsInOrganizationProviderTest
    {
        private PersonsInOrganizationProvider _target;
        private IList<IPerson> _persons;
        private MockRepository _mockRepository;
        private IPersonRepository _personRepository;
        private DateOnlyPeriod _dateTimePeriod;

        [SetUp]
        public void Setup()
        {
            _persons = new List<IPerson> {PersonFactory.CreatePerson("Alan"), PersonFactory.CreatePerson("Bill")};
            _mockRepository = new MockRepository();
            _personRepository = _mockRepository.StrictMock<IPersonRepository>();
            _dateTimePeriod = new DateOnlyPeriod();
        }

        [Test]
        public void VerifyWhenCreatingWithPersonRepositoryGetBackLoadedPersons()
        {
            _target = new PersonsInOrganizationProvider(_personRepository, _dateTimePeriod);

            IList<IPerson> resultList = null;

            using(_mockRepository.Record())
            {
                Expect.Call(_personRepository.FindPeopleInOrganization(_dateTimePeriod, true))
                    .Return(_persons);
            }
            using(_mockRepository.Playback())
            {
                resultList = _target.GetPersons();
            }

            Assert.IsNotNull(_target);
            Assert.AreSame(_persons[0], resultList[0]);
            Assert.AreSame(_persons[1], resultList[1]);
        }

    }
}