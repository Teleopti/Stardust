﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
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


		///<summary>
		/// Set row
		///</summary>
		///<param name="rowNumber"></param>
		///<param name="theRow"></param>
		void SetRow(int rowNumber, IPersonFinderDisplayRow theRow);
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
