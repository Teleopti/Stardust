using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PersonFinderReadOnlyRepository : IPersonFinderReadOnlyRepository
    {
        private readonly ICurrentUnitOfWork _currentUnitOfWork;

        public PersonFinderReadOnlyRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            _currentUnitOfWork = currentUnitOfWork;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
            MessageId = "System.String.Format(System.String,System.Object[])"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
             "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Find(IPersonFinderSearchCriteria personFinderSearchCriteria)
        {
            personFinderSearchCriteria.TotalRows = 0;
            int cultureId = Domain.Security.Principal.TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.LCID;
	        if (personFinderSearchCriteria.TerminalDate < new DateOnly(1753, 1, 1))
		        personFinderSearchCriteria.TerminalDate = new DateOnly(1753, 1, 1);

            var uow = _currentUnitOfWork.Current();
            var result = ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                "exec [ReadModel].PersonFinder @search_string=:search_string, @search_type=:search_type, @leave_after=:leave_after, @start_row =:start_row, @end_row=:end_row, @order_by=:order_by, @sort_direction=:sort_direction, @culture=:culture")
                                                     .SetString("search_string",
                                                                personFinderSearchCriteria.SearchValue)
                                                     .SetString("search_type",
                                                                Enum.GetName(typeof (PersonFinderField),
                                                                             personFinderSearchCriteria.Field))
													 .SetDateOnly("leave_after",
                                                                  personFinderSearchCriteria.TerminalDate)
                                                     .SetInt32("start_row", personFinderSearchCriteria.StartRow)
                                                     .SetInt32("end_row", personFinderSearchCriteria.EndRow)
                                                     .SetInt32("order_by", personFinderSearchCriteria.SortColumn)
                                                     .SetInt32("sort_direction",
                                                               personFinderSearchCriteria.SortDirection)
                                                     .SetInt32("culture", cultureId)
                                                     .SetResultTransformer(
                                                         Transformers.AliasToBean(typeof (PersonFinderDisplayRow)))
                                                     .SetReadOnly(true)
                                                     .List<IPersonFinderDisplayRow>();

            int row = 0;
            foreach (var personFinderDisplayRow in result)
            {
                personFinderSearchCriteria.TotalRows = personFinderDisplayRow.TotalCount;
                personFinderSearchCriteria.SetRow(row, personFinderDisplayRow);
                row++;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
            MessageId = "System.String.Format(System.String,System.Object)")]
        public void UpdateFindPerson(ICollection<Guid> ids)
        {
            string inputIds = String.Join(",", (from p in ids select p.ToString()).ToArray());
            var uow = _currentUnitOfWork.Current();

            ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                string.Format("exec [ReadModel].[UpdateFindPerson] '{0}'", inputIds)).ExecuteUpdate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
            MessageId = "System.String.Format(System.String,System.Object)")]
        public void UpdateFindPersonData(ICollection<Guid> ids)
        {
            string inputIds = String.Join(",", (from p in ids select p.ToString()).ToArray());
            var uow = _currentUnitOfWork.Current();
            ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                string.Format("exec [ReadModel].[UpdateFindPersonData] '{0}'", inputIds)).ExecuteUpdate();
        }
    }

	public class PersonFinderSearchCriteria : IPersonFinderSearchCriteria
	{
		private readonly PersonFinderField _field;
		private readonly string _searchValue;
		private int _pageSize;
		private int _currentPage;
		private readonly IList<IPersonFinderDisplayRow> _displayRows;
		private DateOnly _terminalDate;

		public PersonFinderSearchCriteria(PersonFinderField field, string searchValue, int pageSize, DateOnly terminalDate, int sortColumn, int sortDirection)
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

    public class PersonFinderDisplayRow : IPersonFinderDisplayRow
    {
        public Guid PersonId { get; set; }

        public Guid? TeamId { get; set; }

        public Guid? SiteId { get; set; }

        public Guid BusinessUnitId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmploymentNumber { get; set; }
        public string Note { get; set; }
        public DateTime TerminalDate { get; set; }
        public bool Grayed { get; set; }
        public int TotalCount { get; set; }
        public Int64 RowNumber { get; set; }
    }
}
