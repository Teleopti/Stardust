using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class PersonFinderReadOnlyRepositoryTest : DatabaseTest
    {
        private IPersonFinderReadOnlyRepository _target;

        [Test]
        public void ShouldLoadPersons()
        {
            UnitOfWork.PersistAll();
            SkipRollback();
            var crit = new PersonFinderSearchCriteriaForTest(PersonFinderField.All, "hejhej", 10,
                                                             new DateOnly(2012, 1, 1), 1, 1);
            _target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
            _target.Find(crit);
            Assert.That(crit.TotalRows, Is.EqualTo(0));
        }

		[Test]
		public void ShouldCallUpdateReadModelWithoutCrash()
		{
			UnitOfWork.PersistAll();
			SkipRollback();
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.UpdateFindPerson(new Guid[ ] {Guid.NewGuid()});
		}

		[Test]
		public void ShouldCallUpdateGroupingReadModelGroupPageWithoutCrash()
		{
			UnitOfWork.PersistAll();
			SkipRollback();
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
            _target.UpdateFindPersonData(new Guid[] { Guid.NewGuid() });
		}

		[Test]
		public void ShouldHandleTooSmallDate()
		{
			UnitOfWork.PersistAll();
			SkipRollback();
			var crit = new PersonFinderSearchCriteriaForTest(PersonFinderField.All, "hejhej", 10,
																			 new DateOnly(1012, 1, 1), 1, 1);
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(0));
		}
    }

    internal class PersonFinderSearchCriteriaForTest : IPersonFinderSearchCriteria
    {
        private readonly PersonFinderField _field;
        private readonly string _searchValue;
        private int _pageSize;
        private int _currentPage;
        private readonly IList<IPersonFinderDisplayRow> _displayRows;
        private DateOnly _terminalDate;

        public PersonFinderSearchCriteriaForTest(PersonFinderField field, string searchValue, int pageSize, DateOnly terminalDate, int sortColumn, int sortDirection)
        {
            _field = field;
            _searchValue = searchValue;
            _pageSize = pageSize;
            _displayRows = new List<IPersonFinderDisplayRow>();
            _terminalDate = terminalDate;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
            _currentPage = 1;

            for (var i = 0; i < _pageSize; i++)
            {
                _displayRows.Add(new PersonFinderDisplayRow());
            }
        }

        public PersonFinderField Field
        {
            get { return _field; }
        }

        public string SearchValue
        {
            get { return _searchValue; }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public int CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; }
        }

        public ReadOnlyCollection<IPersonFinderDisplayRow> DisplayRows
        {
            get { return new ReadOnlyCollection<IPersonFinderDisplayRow>(_displayRows); }
        }

        public DateOnly TerminalDate
        {
            get { return _terminalDate; }
			  set { _terminalDate = value; }
        }

        public int TotalPages
        {
            get
            {
                if (TotalRows == 0) return 0;
                return (TotalRows - 1) / _pageSize + 1;
            }
        }

        public int TotalRows { get; set; }

        public int StartRow
        {
            get { return _currentPage * _pageSize - _pageSize + 1; }
        }

        public int EndRow
        {
            get { return StartRow + _pageSize; }
        }

        public void SetRow(int rowNumber, IPersonFinderDisplayRow theRow)
        {
            if (rowNumber >= _displayRows.Count)
                return;
            _displayRows[rowNumber] = theRow;
        }

        public int SortColumn { get; set; }
        public int SortDirection { get; set; }
    }
}