using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Provides data for creation of groupings (GroupPage)
    ///</summary>
    public interface IGroupPageDataProvider
    {
        ///<summary>
        /// Get Person collection
        ///</summary>
        IEnumerable<IPerson> PersonCollection { get; }

        ///<summary>
        /// Get Contract collection
        ///</summary>
        IEnumerable<IContract> ContractCollection { get; }

        ///<summary>
        /// Get ContractSchedule collection
        ///</summary>
        IEnumerable<IContractSchedule> ContractScheduleCollection { get; }

        ///<summary>
        /// Get PartTimePercentage collection
        ///</summary>
        IEnumerable<IPartTimePercentage> PartTimePercentageCollection { get; }

        ///<summary>
        /// Get RuleSetBag collection
        ///</summary>
        IEnumerable<IRuleSetBag> RuleSetBagCollection { get; }

        ///<summary>
        /// Gets the user defined groupings.
        ///</summary>
        IEnumerable<IGroupPage> UserDefinedGroupings { get; }

        ///<summary>
        /// Gets the business unit (this collection should only contain one item!).
        ///</summary>
        IEnumerable<IBusinessUnit> BusinessUnitCollection { get; }

        ///<summary>
        /// Gets the period to base the structure on
        ///</summary>
        DateOnlyPeriod SelectedPeriod { get; }

        /// <summary>
        /// Gets the skill collection.
        /// </summary>
        /// <value>The skill collection.</value>
        IList<ISkill> SkillCollection { get; }
    }
}
