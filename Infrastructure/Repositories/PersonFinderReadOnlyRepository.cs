using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonFinderReadOnlyRepository : IPersonFinderReadOnlyRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public PersonFinderReadOnlyRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		private IEnumerable<string> parse(string searchValue)
		{
			const string quotePattern = "(\"[^\"]*?\")";

			var notParsedSearchValue = Regex.Replace(searchValue, quotePattern, " $1 ");
			notParsedSearchValue = Regex.Replace(notParsedSearchValue, " {2,}", " ");

			const string splitPattern = "[^\\s\"]+|\"[^\"]*\"";
			var matches = Regex.Matches(notParsedSearchValue, splitPattern);
			var result =
				(from object match in matches select match.ToString().Replace("\"", "").Trim())
				.Where(x => !string.IsNullOrEmpty(x));

			return new HashSet<string>(result);
		}

		private string createSearchString(IDictionary<PersonFinderField, string> criterias)
		{
			const char splitter = ';';
			var builder = new StringBuilder();

			foreach (var criteria in criterias)
			{
				var values = parse(criteria.Value)
					.Aggregate("", (current, value) => string.Concat(current, value, splitter))
					.TrimEnd(splitter);

				builder.AppendFormat("{0}:{1},", criteria.Key, values);
			}

			if (builder.Length > 0)
			{
				builder.Length = builder.Length - 1;
			}

			return builder.ToString();
		}

		public void Find(IPersonFinderSearchCriteria personFinderSearchCriteria)
		{
			personFinderSearchCriteria.TotalRows = 0;
			var cultureId = Domain.Security.Principal.TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.LCID;

			var minDate = new DateOnly(1753, 1, 1);
			if (personFinderSearchCriteria.TerminalDate < minDate)
				personFinderSearchCriteria.TerminalDate = minDate;
			var uow = _currentUnitOfWork.Current();

			if (personFinderSearchCriteria.SearchCriterias.Count == 0)
			{
				return;
			}

			var result = ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
				"exec [ReadModel].PersonFinderWithCriteria @search_criterias=:searchCriterias_string, "
				+ "@leave_after=:leave_after, @start_row =:start_row, @end_row=:end_row, @order_by=:order_by, @culture=:culture, @business_unit_id=:business_unit_id, @belongs_to_date=:belongs_to_date")
				.SetString("searchCriterias_string", createSearchString(personFinderSearchCriteria.SearchCriterias))
				.SetDateOnly("leave_after", personFinderSearchCriteria.TerminalDate)
				.SetInt32("start_row", personFinderSearchCriteria.StartRow)
				.SetInt32("end_row", personFinderSearchCriteria.EndRow)
				.SetString("order_by", generateOrderByString(personFinderSearchCriteria.SortColumns))
				.SetInt32("culture", cultureId)
				.SetGuid("business_unit_id", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.SetDateOnly("belongs_to_date", personFinderSearchCriteria.BelongsToDate)
				.SetResultTransformer(Transformers.AliasToBean(typeof(PersonFinderDisplayRow)))
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

		public List<Guid> FindPersonIdsInTeams(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			var result = new List<Guid>();

			if (teamIds.Length == 0)
			{
				return result;
			}

			foreach (var teamIdsBatch in teamIds.Batch(100))
			{
				var teamIdsString = string.Join(",", teamIdsBatch.Select(x => x.ToString()));
				if (searchCriteria.Any())
				{
					var searchString = createSearchString(searchCriteria);
					var batchResult = _currentUnitOfWork.Session().CreateSQLQuery(
							"exec [ReadModel].[PersonFinderWithCriteriaAndTeamsSimplified] @search_criterias=:search_criterias, @belongs_to_date=:belongs_to_date, @team_ids=:team_ids")
						.SetString("search_criterias", searchString)
						.SetDateOnly("belongs_to_date", date)
						.SetString("team_ids", teamIdsString)
						.SetReadOnly(true)
						.List<Guid>();

					result.AddRange(batchResult);
				}
				else
				{
					result.AddRange(findPersonIdsInTeams(new DateOnlyPeriod(date, date), teamIdsString));
				}
			}

			return result;
		}


		public List<Guid> FindPersonIdsInTeamsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			var result = new List<Guid>();

			if (teamIds.Length == 0)
			{
				return result;
			}

			foreach (var teamIdsBatch in teamIds.Batch(100))
			{
				var teamIdsString = string.Join(",", teamIdsBatch.Select(x => x.ToString()));
				if (searchCriteria.Any())
				{
					var searchString = createSearchString(searchCriteria);
					var batchResult = _currentUnitOfWork.Session().CreateSQLQuery(
							"exec [ReadModel].[PersonFinderWithCriteriaAndTeamsSimplifiedBasedOnRecentPeriod] @search_criterias=:search_criterias, @start_date=:start_date, @end_date=:end_date, @team_ids=:team_ids")
						.SetString("search_criterias", searchString)
						.SetDateOnly("start_date", period.StartDate)
						.SetDateOnly("end_date", period.EndDate)
						.SetString("team_ids", teamIdsString)
						.SetReadOnly(true)
						.List<Guid>();

					result.AddRange(batchResult);
				}
				else
				{
					result.AddRange(findPersonIdsInTeams(period, teamIdsString));
				}
			}

			return result;
		}

		private IList<Guid> findPersonIdsInTeams(DateOnlyPeriod period, string teamIdsString)
		{
			return _currentUnitOfWork.Session()
				.CreateSQLQuery(
					"exec [dbo].[PersonInTeams] @start_date=:start_date, @end_date=:end_date, @team_ids=:team_ids")
				.SetDateOnly("start_date", period.StartDate)
				.SetDateOnly("end_date", period.EndDate)
				.SetString("team_ids", teamIdsString)
				.SetReadOnly(true)
				.List<Guid>();

		}

		public void FindInTeams(IPersonFinderSearchCriteria personFinderSearchCriteria, Guid[] teamIds)
		{
			personFinderSearchCriteria.TotalRows = 0;
			var cultureId = Domain.Security.Principal.TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.LCID;
			if (teamIds.Length == 0)
			{
				return;
			}

			var minDate = new DateOnly(1753, 1, 1);
			if (personFinderSearchCriteria.TerminalDate < minDate)
				personFinderSearchCriteria.TerminalDate = minDate;

			var result = new List<IPersonFinderDisplayRow>();

			var orderByString = generateOrderByString(personFinderSearchCriteria.SortColumns);
			var searchString = createSearchString(personFinderSearchCriteria.SearchCriterias);
			foreach (var teamIdsBatch in teamIds.Batch(100))
			{

				var teamIdsString = string.Join(",", teamIdsBatch.Select(x => x.ToString()));
				var batchResult = _currentUnitOfWork.Session().CreateSQLQuery(
						"exec [ReadModel].[PersonFinderWithCriteriaAndTeams] @search_criterias=:searchCriterias_string, "
						+
						"@leave_after=:leave_after, @start_row =:start_row, @end_row=:end_row, @order_by=:order_by, @culture=:culture, @business_unit_id=:business_unit_id, @belongs_to_date=:belongs_to_date, @teamIds=:teamIds")
					.SetString("searchCriterias_string", searchString)
					.SetDateOnly("leave_after", personFinderSearchCriteria.TerminalDate)
					.SetInt32("start_row", personFinderSearchCriteria.StartRow)
					.SetInt32("end_row", personFinderSearchCriteria.EndRow)
					.SetString("order_by", orderByString)
					.SetInt32("culture", cultureId)
					.SetGuid("business_unit_id", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
					.SetDateOnly("belongs_to_date", personFinderSearchCriteria.BelongsToDate)
					.SetString("teamIds", teamIdsString)
					.SetResultTransformer(Transformers.AliasToBean(typeof(PersonFinderDisplayRow)))
					.SetReadOnly(true)
					.List<IPersonFinderDisplayRow>();
				result.AddRange(batchResult);
			}

			int row = 0;
			foreach (var personFinderDisplayRow in result)
			{
				personFinderSearchCriteria.TotalRows = personFinderDisplayRow.TotalCount;
				personFinderSearchCriteria.SetRow(row, personFinderDisplayRow);
				row++;
			}
		}

		private string generateOrderByString(IDictionary<string, bool> sortColumns)
		{
			if (sortColumns == null || !sortColumns.Any())
			{
				return "1:1";
			}

			// This mapping should keep exact same as in SP "ReadModel.PersonFinderWithCriteria"
			var columnMapping = new Dictionary<string, int>
			{
				{"firstname", 0},
				{"lastname", 1},
				{"employmentnumber", 2},
				{"note", 3},
				{"terminaldate", 4}
			};

			var orderBy = new StringBuilder();

			var terminalDateExist = false;
			foreach (var col in sortColumns)
			{
				var columnName = col.Key.Trim().ToLower();
				if (columnMapping.ContainsKey(columnName))
				{
					orderBy.Append($"{columnMapping[columnName]}:{(col.Value ? 1 : 0)},");
				}
				else if (!terminalDateExist)
				{
					orderBy.Append($"{columnMapping["terminaldate"]}:{(col.Value ? 1 : 0)},");
					terminalDateExist = true;
				}
			}

			if (orderBy.Length > 0)
			{
				orderBy.Length = orderBy.Length - 1;
			}

			return orderBy.ToString();
		}

		public void FindPeople(IPeoplePersonFinderSearchCriteria personFinderSearchCriteria)
		{
			personFinderSearchCriteria.TotalRows = 0;
			int cultureId = Domain.Security.Principal.TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.LCID;
			if (personFinderSearchCriteria.TerminalDate < new DateOnly(1753, 1, 1))
				personFinderSearchCriteria.TerminalDate = new DateOnly(1753, 1, 1);

			var uow = _currentUnitOfWork.Current();
			var result = ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
				"exec [ReadModel].PersonFinder @search_string=:search_string, @search_type=:search_type, "
				+ "@leave_after=:leave_after, @start_row =:start_row, @end_row=:end_row, @order_by=:order_by, "
				+ "@sort_direction=:sort_direction, @culture=:culture")
				.SetString("search_string", personFinderSearchCriteria.SearchValue)
				.SetString("search_type", Enum.GetName(typeof(PersonFinderField), personFinderSearchCriteria.Field))
				.SetDateOnly("leave_after", personFinderSearchCriteria.TerminalDate)
				.SetInt32("start_row", personFinderSearchCriteria.StartRow)
				.SetInt32("end_row", personFinderSearchCriteria.EndRow)
				.SetInt32("order_by", personFinderSearchCriteria.SortColumn)
				.SetInt32("sort_direction", personFinderSearchCriteria.SortDirection)
				.SetInt32("culture", cultureId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(PersonFinderDisplayRow)))
				.SetReadOnly(true)
				.List<IPersonFinderDisplayRow>();

			var row = 0;
			foreach (var personFinderDisplayRow in result)
			{
				personFinderSearchCriteria.TotalRows = personFinderDisplayRow.TotalCount;
				personFinderSearchCriteria.SetRow(row, personFinderDisplayRow);
				row++;
			}
		}

		public void UpdateFindPerson(ICollection<Guid> ids)
		{
			string inputIds = string.Join(",", from p in ids select p.ToString());
			var uow = _currentUnitOfWork.Current();

			((NHibernateUnitOfWork)uow).Session.CreateSQLQuery("exec [ReadModel].[UpdateFindPerson] :inputIds")
				.SetString("inputIds", inputIds)
				.ExecuteUpdate();
		}

		public void UpdateFindPersonData(ICollection<Guid> ids)
		{
			string inputIds = string.Join(",", from p in ids select p.ToString());
			var uow = _currentUnitOfWork.Current();
			((NHibernateUnitOfWork)uow).Session.CreateSQLQuery("exec [ReadModel].[UpdateFindPersonData] :inputIds")
				.SetString("inputIds", inputIds)
				.ExecuteUpdate();
		}
	}
	public class PeoplePersonFinderSearchCriteria : IPeoplePersonFinderSearchCriteria
	{
		private readonly IList<IPersonFinderDisplayRow> _displayRows;

		public PeoplePersonFinderSearchCriteria(PersonFinderField field, string searchValue, int pageSize,
			DateOnly terminalDate, int sortColumn, int sortDirection)
		{
			Field = field;
			SearchValue = searchValue;
			PageSize = pageSize;
			_displayRows =
				Enumerable.Range(0, pageSize).Select<int, IPersonFinderDisplayRow>(i => new PersonFinderDisplayRow()).ToList();
			TerminalDate = terminalDate;
			SortColumn = sortColumn;
			SortDirection = sortDirection;
			CurrentPage = 1;
		}

		public PersonFinderField Field { get; }

		public string SearchValue { get; }

		public int PageSize { get; set; }

		public int CurrentPage { get; set; }

		public ReadOnlyCollection<IPersonFinderDisplayRow> DisplayRows => new ReadOnlyCollection<IPersonFinderDisplayRow>(_displayRows);

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

		public void SetRow(int rowNumber, IPersonFinderDisplayRow theRow)
		{
			if (rowNumber >= _displayRows.Count)
				return;
			_displayRows[rowNumber] = theRow;
		}

		public int SortColumn { get; set; }
		public int SortDirection { get; set; }
	}

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

		public ReadOnlyCollection<IPersonFinderDisplayRow> DisplayRows => new ReadOnlyCollection<IPersonFinderDisplayRow>(_displayRows);

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
	}
}
