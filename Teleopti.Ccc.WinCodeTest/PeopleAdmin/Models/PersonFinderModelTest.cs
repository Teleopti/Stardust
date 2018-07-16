using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class PersonFinderModelTest
    {
        private MockRepository _mocks;
        private IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;
        private PersonFinderModel _target;
		private IPeoplePersonFinderSearchCriteria _searchCriteria;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _searchCriteria = _mocks.StrictMock<IPeoplePersonFinderSearchWithPermissionCriteria>();
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
			_searchCriteria = new PeoplePersonFinderSearchCriteria(PersonFinderField.All, "aa", 5,
                                                             new DateOnly(DateTime.Today.AddMonths(-2)),2,0);
			_searchCriteria.SetRow(new PersonFinderDisplayRow { BusinessUnitId = Guid.NewGuid(), PersonId = Guid.Empty });
			_searchCriteria.SetRow(new PersonFinderDisplayRow{BusinessUnitId = Guid.Empty, PersonId = Guid.NewGuid()});
            _target = new PersonFinderModel(_personFinderReadOnlyRepository, _searchCriteria);
            
            Expect.Call(() => _personFinderReadOnlyRepository.FindPeople(_searchCriteria));
            _mocks.ReplayAll();
            _target.Find();
			Assert.That(_searchCriteria.DisplayRows[0].Grayed, Is.True);
            _mocks.VerifyAll();
        }

		[Ignore("Bug76798 to be fixed")]
		[Test]
		public void ShouldSort(
			[Values(0, 1, 2, 3, 4)] int sortColumn, //firstname = 0, lastname = 1, employmentnumber = 2, note = 3, terminaldate = 4
			[Values(0, 1)] int sortDirection)
		{

			var searchCritiera = new PeoplePersonFinderSearchCriteria(PersonFinderField.All, "_", 1, DateOnly.Today, sortColumn, sortDirection);
			var row1 = new PersonFinderDisplayRow {FirstName = "a", LastName = "a", EmploymentNumber = "1", Note = "a", TerminalDate = DateTime.Now};
			var row2 = new PersonFinderDisplayRow { FirstName = "b", LastName = "b", EmploymentNumber = "2", Note = "b", TerminalDate = DateTime.Now.AddDays(1) };
			var row3 = new PersonFinderDisplayRow { FirstName = "c", LastName = "c", EmploymentNumber = "3", Note = "c", TerminalDate = DateTime.Now.AddDays(2) };
			searchCritiera.SetRow(row3);
			searchCritiera.SetRow(row1);
			searchCritiera.SetRow(row2);
			_target = new PersonFinderModel(_personFinderReadOnlyRepository, searchCritiera);

			//_target.Sort();

			if (sortDirection == 1)
			{
				_target.SearchCriteria.DisplayRows[0].Should().Be.EqualTo(row1);
				_target.SearchCriteria.DisplayRows[1].Should().Be.EqualTo(row2);
				_target.SearchCriteria.DisplayRows[2].Should().Be.EqualTo(row3);
			}
			else
			{
				_target.SearchCriteria.DisplayRows[0].Should().Be.EqualTo(row3);
				_target.SearchCriteria.DisplayRows[1].Should().Be.EqualTo(row2);
				_target.SearchCriteria.DisplayRows[2].Should().Be.EqualTo(row1);
			}
		}
    }
}
