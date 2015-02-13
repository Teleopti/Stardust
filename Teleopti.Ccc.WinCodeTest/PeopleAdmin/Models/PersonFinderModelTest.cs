using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
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
			_searchCriteria.SetRow(0, new PersonFinderDisplayRow { BusinessUnitId = Guid.NewGuid(), PersonId = Guid.Empty });
			_searchCriteria.SetRow(1, new PersonFinderDisplayRow{BusinessUnitId = Guid.Empty, PersonId = Guid.NewGuid()});
            _target = new PersonFinderModel(_personFinderReadOnlyRepository, _searchCriteria);
            
            Expect.Call(() => _personFinderReadOnlyRepository.Find(_searchCriteria));
            _mocks.ReplayAll();
            _target.Find();
			Assert.That(_searchCriteria.DisplayRows[0].Grayed, Is.True);
            _mocks.VerifyAll();
        }   
    }
}
