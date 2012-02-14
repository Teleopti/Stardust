using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class PersonFinderModelTest
    {
        private MockRepository _mocks;
        private IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;
        private PersonFinderModel _target;
        private IPersonFinderSearchCriteria _searchCriteria;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _searchCriteria = _mocks.StrictMock<IPersonFinderSearchCriteria>();
            _personFinderReadOnlyRepository = _mocks.StrictMock<IPersonFinderReadOnlyRepository>();
            _target = new PersonFinderModel(_personFinderReadOnlyRepository, _searchCriteria);
        }

        [Test]
        public void ShouldContainEmptySearchCriteria()
        {
            Assert.That(_target.SearchCriteria, Is.EqualTo(_searchCriteria) );
        }

        [Test]
        public void ShouldCallRepositoryOnFind()
        {
            _searchCriteria = new PersonFinderSearchCriteria(PersonFinderField.All, "aa", 5,
                                                             new DateOnly(DateTime.Today.AddMonths(-2)),2,0);
            _target = new PersonFinderModel(_personFinderReadOnlyRepository, _searchCriteria);
            
            Expect.Call(() => _personFinderReadOnlyRepository.Find(_searchCriteria));
            _mocks.ReplayAll();
            _target.Find();
            _mocks.VerifyAll();
        }   
    }
}
