using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch
{
	public class PersonFinderSearchCriteria : IPersonFinderSearchCriteria
	{
		private readonly List<IPersonFinderDisplayRow> _displayRows;

		public PersonFinderSearchCriteria(PersonFinderField field, string searchValue, int pageSize, DateOnly terminalDate,
			IDictionary<string, bool> sortColumns, DateOnly belongsToDate)
			: this(
				new Dictionary<PersonFinderField, string> { { field, searchValue } }, pageSize, terminalDate, sortColumns, belongsToDate
			)
		{
		}

		public PersonFinderSearchCriteria(IDictionary<PersonFinderField, string> searchCriterias, int pageSize,
			DateOnly terminalDate, IDictionary<string, bool> sortColumns, DateOnly belongsToDate)
		{
			SearchCriterias = searchCriterias;
			PageSize = pageSize;
			_displayRows = Enumerable.Range(0, pageSize).Select<int, IPersonFinderDisplayRow>(i => new PersonFinderDisplayRow()).ToList();
			TerminalDate = terminalDate;
			SortColumns = sortColumns;
			CurrentPage = 1;

			BelongsToDate = belongsToDate;
		}

		public IDictionary<PersonFinderField, string> SearchCriterias { get; }

		public int PageSize { get; set; }

		public int CurrentPage { get; set; }

		public IPersonFinderDisplayRow[] DisplayRows => _displayRows.ToArray();

		public DateOnly TerminalDate { get; set; }

		public int TotalPages
		{
			get
			{
				if (TotalRows == 0) return 0;
				return (TotalRows - 1) / PageSize + 1;
			}
		}

		public int TotalRows { get; set; }

		public int StartRow => CurrentPage * PageSize - PageSize + 1;

		public int EndRow => StartRow + PageSize;

		public DateOnly BelongsToDate { get; set; }

		public void SetRow(int rowNumber, IPersonFinderDisplayRow theRow)
		{
			if (rowNumber >= _displayRows.Count)
				return;
			_displayRows[rowNumber] = theRow;
		}

		public IDictionary<string, bool> SortColumns { get; set; }
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
		public string SiteName { get; set; }
		public string TeamName { get; set; }
	}
}