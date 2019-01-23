using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

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

		public IList<PersonIdentityMatchResult> FindPersonByIdentities(IEnumerable<string> identities)
		{
			const int batchSize = 100;

			var result = new List<PersonIdentityMatchResult>();
			if (identities == null) return result;

			var identityList = identities.ToList();
			if (!identityList.Any()) return result;

			var uow = _currentUnitOfWork.Current();
			var businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current();

			foreach (var identitiesInBatch in identityList.Batch(batchSize))
			{
				var sql = $@"Select EmploymentNumber as [Identity], PersonId as PersonId, {(int)IdentityMatchField.EmploymentNumber} as MatchField
							     From ReadModel.FindPerson
							    Where BusinessUnitId = '{businessUnit.Id}'
							      And EmploymentNumber in (:identities)
							   Union
							   Select Distinct UserCode, PersonId, {(int)IdentityMatchField.ExternalLogon}
							     From ReadModel.ExternalLogon
							    Where Deleted = 0
							      And UserCode in (:identities)";
				var batchResult = ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(sql)
					.AddScalar("Identity", NHibernateUtil.String)
					.AddScalar("PersonId", NHibernateUtil.Guid)
					.AddScalar("MatchField", NHibernateUtil.Int32)
					.SetParameterList("identities", identitiesInBatch.ToArray())
					.SetReadOnly(true)
					.List<object[]>()
					.Select(x => new PersonIdentityMatchResult
					{
						LogonName = (string)x[0],
						PersonId = (Guid)x[1],
						MatchField = (IdentityMatchField)(int)x[2]
					}).ToList();

				result.AddRange(batchResult);
			}

			return result;
		}

		public void Find(IPersonFinderSearchCriteria personFinderSearchCriteria)
		{
			personFinderSearchCriteria.TotalRows = 0;
			var cultureId = Domain.Security.Principal.TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture.LCID;

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
				if (searchCriteria!=null && searchCriteria.Any())
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
			var cultureId = Domain.Security.Principal.TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture.LCID;
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
			int cultureId = Domain.Security.Principal.TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture.LCID;
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

			personFinderSearchCriteria.SetRows(result.ToList());
			personFinderSearchCriteria.TotalRows = personFinderSearchCriteria.DisplayRows.FirstOrDefault()?.TotalCount ?? 0;
		}

		public void FindPeopleWithDataPermission(IPeoplePersonFinderSearchWithPermissionCriteria personFinderSearchCriteria)
		{
			int cultureId = Domain.Security.Principal.TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture.LCID;
			if (personFinderSearchCriteria.TerminalDate < new DateOnly(1753, 1, 1))
				personFinderSearchCriteria.TerminalDate = new DateOnly(1753, 1, 1);

			var uow = _currentUnitOfWork.Current();
			var result = ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
					"exec [ReadModel].[PersonFinderWithDataPermission] @search_string=:search_string, @search_type=:search_type, "
					+ "@leave_after=:leave_after, @start_row =:start_row, @end_row=:end_row, @order_by=:order_by, "
					+ "@sort_direction=:sort_direction, @culture=:culture, " 
					+ "@perm_date=:perm_date, @perm_userid=:perm_userid, @perm_foreignId=:perm_foreignId, @can_see_users=:can_see_users, "
					+ "@current_business_unit=:current_business_unit")
				.SetString("search_string", personFinderSearchCriteria.SearchValue)
				.SetString("search_type", Enum.GetName(typeof(PersonFinderField), personFinderSearchCriteria.Field))
				.SetDateOnly("leave_after", personFinderSearchCriteria.TerminalDate)
				.SetInt32("start_row", personFinderSearchCriteria.StartRow)
				.SetInt32("end_row", personFinderSearchCriteria.EndRow)
				.SetInt32("order_by", personFinderSearchCriteria.SortColumn)
				.SetInt32("sort_direction", personFinderSearchCriteria.SortDirection)
				.SetInt32("culture", cultureId)
				.SetDateOnly("perm_date", personFinderSearchCriteria.PermissionDate)
				.SetGuid("perm_userid", personFinderSearchCriteria.PermissionUserId)
				.SetString("perm_foreignId", personFinderSearchCriteria.PermissionAppFuncForeignId)
				.SetBoolean("can_see_users", personFinderSearchCriteria.CanSeeUsers)
				.SetGuid("current_business_unit", personFinderSearchCriteria.CurrentBusinessUnit)
				.SetResultTransformer(Transformers.AliasToBean(typeof(PersonFinderDisplayRow)))
				.SetReadOnly(true)
				.List<IPersonFinderDisplayRow>();

			personFinderSearchCriteria.SetRows(result.ToList());
			personFinderSearchCriteria.TotalRows = personFinderSearchCriteria.DisplayRows.FirstOrDefault()?.TotalCount ?? 0;
		}

		public bool ValidatePersonIds(List<Guid> ids, DateOnly date, Guid userId, string appFuncForeginId)
		{
			var uow = _currentUnitOfWork.Current();
			var result = ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
					"exec [dbo].[ValidateDataPermissions] @idsString=:idsString, @date=:date, @userId=:userId, @appFuncForeginId=:appFuncForeginId")
				.SetString("idsString", String.Join(",", ids))
				.SetDateOnly("date", date)
				.SetGuid("userId", userId)
				.SetString("appFuncForeginId", appFuncForeginId)
				.SetReadOnly(true)
				.UniqueResult<bool>();

			return result;
		}

		public void UpdateFindPerson(ICollection<Guid> ids)
		{
			if (ids.Count == 1 && ids.First() == Guid.Empty)
				throw new NotSupportedException("Not allowed to rebuild the entire search index from code.");

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

		public List<Guid> FindPersonIdsInGroupsBasedOnPersonPeriod(DateOnlyPeriod period,
			Guid[] groupIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			var result = new List<Guid>();

			if (groupIds.Length == 0)
			{
				return result;
			}

			foreach (var groupBatch in groupIds.Batch(100))
			{
				var valuesString = string.Join(",", groupBatch.Select(x => x.ToString()));

				var searchString = createSearchString(searchCriteria);
				var batchResult = _currentUnitOfWork.Session().CreateSQLQuery(
						"exec [ReadModel].[PersonFinderWithCriteriaAndGroupsBasedOnRecentPeriod] @business_unit_id=:business_unit_id, @search_criterias=:search_criterias, @start_date=:start_date, @end_date=:end_date, @group_ids=:group_ids")
					.SetGuid("business_unit_id", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
					.SetString("search_criterias", searchString)
					.SetDateOnly("start_date", period.StartDate)
					.SetDateOnly("end_date", period.EndDate)
					.SetString("group_ids", valuesString)
					.SetReadOnly(true)
					.List<Guid>();
				result.AddRange(batchResult);
			}
			return result;
		}

		public List<Guid> FindPersonIdsInDynamicOptionalGroupPages(DateOnlyPeriod period, Guid groupPageId,
			string[] dynamicValues, IDictionary<PersonFinderField, string> searchCriteria)
		{
			var result = new List<Guid>();

			if (dynamicValues.Length == 0)
			{
				return result;
			}

			foreach (var groupBatch in dynamicValues.Batch(100))
			{
				var valuesString = string.Join(",", groupBatch.Select(x => x.ToString()));

				var searchString = createSearchString(searchCriteria);
				var batchResult = _currentUnitOfWork.Session().CreateSQLQuery(
						"exec [ReadModel].[PersonFinderWithCriteriaAndDynamicOptionalGroupsBasedOnRecentPeriod] @business_unit_id=:business_unit_id, @search_criterias=:search_criterias, @start_date=:start_date, @end_date=:end_date, @group_page_id=:group_page_id, @dynamic_values=:dynamic_values")
					.SetGuid("business_unit_id", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
					.SetString("search_criterias", searchString)
					.SetDateOnly("start_date", period.StartDate)
					.SetDateOnly("end_date", period.EndDate)
					.SetGuid("group_page_id", groupPageId)
					.SetString("dynamic_values", valuesString)
					.SetReadOnly(true)
					.List<Guid>();
				result.AddRange(batchResult);
			}
			return result;
		}
	}
	public class PeoplePersonFinderSearchCriteria : IPeoplePersonFinderSearchCriteria
	{
		private List<IPersonFinderDisplayRow> _displayRows;

		public PeoplePersonFinderSearchCriteria(PersonFinderField field, string searchValue, int pageSize,
			DateOnly terminalDate, int sortColumn, int sortDirection)
		{
			Field = field;
			SearchValue = searchValue;
			PageSize = pageSize;
			_displayRows = new List<IPersonFinderDisplayRow>(); //Enumerable.Range(0, pageSize).Select<int, IPersonFinderDisplayRow>(i => new PersonFinderDisplayRow()).ToList();
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

		public void SetRow(IPersonFinderDisplayRow theRow)
		{
			_displayRows.Add(theRow);
		}

		public int SortColumn { get; set; }
		public int SortDirection { get; set; }

		public void SetRows(IEnumerable<IPersonFinderDisplayRow> rows)
		{
			_displayRows.AddRange(rows);
		}

		public void ClearResult()
		{
			_displayRows.Clear();
		}
	}

	public class PeoplePersonFinderSearchWithPermissionCriteria : PeoplePersonFinderSearchCriteria, IPeoplePersonFinderSearchWithPermissionCriteria
	{
		public PeoplePersonFinderSearchWithPermissionCriteria(
			PersonFinderField field, 
			string searchValue, 
			int currentPage,
			int pageSize, 
			DateOnly terminalDate, 
			int sortColumn, 
			int sortDirection, 
			DateOnly permissionDate, 
			Guid permissionUserId, 
			string permissionAppFuncForeignId,
			bool canSeeUsers,
			Guid currentBusinessUnit) : 
				base(field, searchValue, pageSize, terminalDate, sortColumn, sortDirection)
		{
			this.CurrentPage = currentPage;
			this.PermissionDate = permissionDate;
			this.PermissionUserId = permissionUserId;
			this.PermissionAppFuncForeignId = permissionAppFuncForeignId;
			this.CanSeeUsers = canSeeUsers;
			this.CurrentBusinessUnit = currentBusinessUnit;
		}

		public bool CanSeeUsers { get; set; }
		public DateOnly PermissionDate { get; set; }
		public Guid PermissionUserId { get; set; }
		public string PermissionAppFuncForeignId { get; set; }
		public Guid CurrentBusinessUnit { get; set; }
	}
}
