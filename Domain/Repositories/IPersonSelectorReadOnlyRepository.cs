using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPersonSelectorReadOnlyRepository
    {
        IList<IPersonSelectorOrganization> GetOrganization(DateOnlyPeriod dateOnlyPeriod, bool loadUsers);
        IList<IPersonSelectorBuiltIn> GetBuiltIn(DateOnlyPeriod dateOnlyPeriod, PersonSelectorField loadType);
        IList<IPersonSelectorUserDefined> GetUserDefinedTab(DateOnly onDate, Guid value);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IList<IUserDefinedTabLight> GetUserDefinedTabs();
	    IList<IAuthorizeOrganisationDetail> GetPersonForShiftTrade(DateOnly shiftTradeDate, Guid? teamId);
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
        Note
    }

    public interface IPersonSelectorOrganization : IAuthorizeOrganisationDetail, ILightPerson
    {
        string Team { get; set; }
        string Site { get; set; }
    }

    public interface IPersonSelectorBuiltIn : IAuthorizeOrganisationDetail, ILightPerson
    {
        string Node { get; set; }
    }

    public interface IPersonSelectorUserDefined : IAuthorizeOrganisationDetail, ILightPerson
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