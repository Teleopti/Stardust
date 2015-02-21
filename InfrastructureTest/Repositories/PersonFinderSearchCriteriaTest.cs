using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class PersonFinderSearchCriteriaTest
	{
		private PersonFinderSearchCriteria _target;
		private PersonFinderField _field;
		private string _searchValue;
		private int _pageSize;
		private int _currentPage;
		private int _totalRows;
		private DateOnly _terminalDate;

		[SetUp]
		public void Setup()
		{
			_field = PersonFinderField.All;
			_searchValue = "searchValue";
			_pageSize = 9;
			_currentPage = 2;
			_totalRows = 11;
			_terminalDate = new DateOnly(2011, 2, 2);
			_target = new PersonFinderSearchCriteria(_field, _searchValue, _pageSize, _terminalDate, 2, 0) { TotalRows = _totalRows };
			_target.CurrentPage = _currentPage;
		}

		[Test]
		public void ShouldConstructProperties()
		{
			Assert.AreEqual(_field, _target.Field);
			Assert.AreEqual(_searchValue, _target.SearchValue);
			Assert.AreEqual(_pageSize, _target.PageSize);
			Assert.AreEqual(_currentPage, _target.CurrentPage);
			Assert.AreEqual(_pageSize, _target.DisplayRows.Count);
			Assert.AreEqual(_totalRows, _target.TotalRows);
			Assert.AreEqual(_terminalDate, _target.TerminalDate);
		}

		[Test]
		public void ShouldSetCurrentPage()
		{
			Assert.AreEqual(2, _target.CurrentPage);
		}

		[Test]
		public void ShouldGetStartRow()
		{
			Assert.AreEqual(10, _target.StartRow);
		}

		[Test]
		public void ShouldGetEndRow()
		{
			Assert.AreEqual(19, _target.EndRow);
		}

		[Test]
		public void ShouldGetTotalPages()
		{
			Assert.AreEqual(2, _target.TotalPages);

			_target.TotalRows = 0;
			Assert.AreEqual(0, _target.TotalPages);
		}

		[Test]
		public void ShouldSetPageSize()
		{
			_target.PageSize = 9;
			Assert.AreEqual(9, _target.PageSize);
		}
	}
}