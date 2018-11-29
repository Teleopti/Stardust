using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonSelectorReadOnlyRepository
	{
		IList<IPersonSelectorOrganization> GetOrganization(DateOnlyPeriod dateOnlyPeriod, bool loadUsers);
		IList<IPersonSelectorOrganization> GetOrganizationForWeb(DateOnlyPeriod dateOnlyPeriod);
		IList<IPersonSelectorBuiltIn> GetBuiltIn(DateOnlyPeriod dateOnlyPeriod, PersonSelectorField loadType, Guid optionalColumnId);
		IList<IPersonSelectorUserDefined> GetUserDefinedTab(DateOnly onDate, Guid value);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IList<IUserDefinedTabLight> GetUserDefinedTabs();

		IList<IUserDefinedTabLight> GetOptionalColumnTabs();

	}

	public enum PersonSelectorField
	{
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
		///<summary>
		///</summary>
		Note,

		OptionalColumn
	}

	public interface IPersonSelectorOrganization : IPersonAuthorization, ILightPerson
	{
		string Team { get; set; }
		string Site { get; set; }
	}

	public interface IPersonSelectorBuiltIn : IPersonAuthorization, ILightPerson
	{
		string Node { get; set; }
	}

	public interface IPersonSelectorUserDefined : IPersonAuthorization, ILightPerson
	{
		Guid NodeId { get; set; }
		string Node { get; set; }
		Guid? ParentId { get; set; }
		int Level { get; set; }
		bool Show { get; set; }
	}

	public interface IUserDefinedTabLight
	{
		Guid Id { get; set; }
		string Name { get; set; }
	}
}