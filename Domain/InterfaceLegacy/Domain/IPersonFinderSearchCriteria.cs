using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	// Used for translation auto search, DO NOT REMOVE!
	// UserTexts.Resources.PersonFinderFieldAll
	// UserTexts.Resources.PersonFinderFieldBudgetGroup
	// UserTexts.Resources.PersonFinderFieldContract
	// UserTexts.Resources.PersonFinderFieldContractSchedule
	// UserTexts.Resources.PersonFinderFieldEmploymentNumber
	// UserTexts.Resources.PersonFinderFieldFirstName
	// UserTexts.Resources.PersonFinderFieldLastName
	// UserTexts.Resources.PersonFinderFieldNote
	// UserTexts.Resources.PersonFinderFieldOrganization
	// UserTexts.Resources.PersonFinderFieldPartTimePercentage
	// UserTexts.Resources.PersonFinderFieldRole
	// UserTexts.Resources.PersonFinderFieldSkill
	// UserTexts.Resources.PersonFinderFieldShiftBag
	
	/// <summary>
	/// Enum for field to search in with PersonFinder
	/// </summary>
	public enum PersonFinderField
	{
		/// <summary>
		/// All
		/// </summary>
		All,
		/// <summary>
		/// Skill
		/// </summary>
		Skill,
		/// <summary>
		/// Contract
		/// </summary>
		Contract,
		/// <summary>
		/// Organization
		/// </summary>
		Organization,
		/// <summary>
		/// ContractSchedule
		/// </summary>
		ContractSchedule,
		/// <summary>
		/// PartTimePercentage
		/// </summary>
		PartTimePercentage,
		/// <summary>
		/// ShiftBag
		/// </summary>
		ShiftBag,
		/// <summary>
		/// BudgetGroup
		/// </summary>
		BudgetGroup,
		Role,
		FirstName,
		LastName,
		EmploymentNumber,
		Note
	}

	public interface IPeoplePersonFinderSearchCriteria
	{
		/// <summary>
		/// Field to search in
		/// </summary>
		PersonFinderField Field { get; }
		/// <summary>
		/// Search value
		/// </summary>
		string SearchValue { get; }
		/// <summary>
		/// Page size
		/// </summary>
		int PageSize { get; set; }
		/// <summary>
		/// Current page
		/// </summary>
		int CurrentPage { get; set; }
		/// <summary>
		/// Row to fill with result
		/// </summary>
		ReadOnlyCollection<IPersonFinderDisplayRow> DisplayRows { get; }
		/// <summary>
		/// Total pages
		/// </summary>
		int TotalPages { get; }
		///<summary>
		///</summary>
		int TotalRows { get; set; }
		/// <summary>
		/// Terminal date
		/// </summary>
		DateOnly TerminalDate { get; set; }
		/// <summary>
		/// Start row
		/// </summary>
		int StartRow { get; }
		/// <summary>
		/// End row
		/// </summary>
		int EndRow { get; }

		///<summary>
		///</summary>
		int SortColumn { get; set; }

		///<summary>
		///</summary>
		int SortDirection { get; set; }

		/// <summary>
		///  Set row
		/// </summary>
		/// <param name="theRow"></param>
		void SetRow(IPersonFinderDisplayRow theRow);

		void SetRows(IEnumerable<IPersonFinderDisplayRow> rows);
		void ClearResult();
	}

	public interface IPeoplePersonFinderSearchWithPermissionCriteria : IPeoplePersonFinderSearchCriteria
	{
		/// <summary>
		/// The date to use when checking data permission
		/// </summary>
		DateOnly PermissionDate { get; set; }
		
		/// <summary>
		/// The user to check data permission for when searching
		/// </summary>
		Guid PermissionUserId { get; set; }

		/// <summary>
		/// The ApplicationFunction ForeignId to use when checking data permission on searchresult
		/// </summary>
		string PermissionAppFuncForeignId { get; set; }

		bool CanSeeUsers { get; set; }

		Guid CurrentBusinessUnit { get; set; }
	}

	/// <summary>
	/// Search criteria for PersonFinder
	/// </summary>
	public interface IPersonFinderSearchCriteria
	{
		/// <summary>
		/// Page size
		/// </summary>
		int PageSize { get; set; }
		/// <summary>
		/// Current page
		/// </summary>
		int CurrentPage { get; set; }
		/// <summary>
		/// Row to fill with result
		/// </summary>
		IPersonFinderDisplayRow[] DisplayRows { get; }
		/// <summary>
		/// Total pages
		/// </summary>
		int TotalPages { get; }
		///<summary>
		///</summary>
		int TotalRows { get; set; }
		/// <summary>
		/// Terminal date
		/// </summary>
		DateOnly TerminalDate { get; set; }
		/// <summary>
		/// Start row
		/// </summary>
		int StartRow { get; }
		/// <summary>
		/// End row
		/// </summary>
		int EndRow { get; }

		DateOnly BelongsToDate { get; set; }

		///<summary>
		/// Set row
		///</summary>
		///<param name="rowNumber"></param>
		///<param name="theRow"></param>
		void SetRow(int rowNumber, IPersonFinderDisplayRow theRow);

		IDictionary<PersonFinderField, string> SearchCriterias { get; }

		IDictionary<string, bool> SortColumns { get; set; }
	}
}
