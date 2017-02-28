using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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

	    /// <summary>
	    ///  Gets the user defined groupings.
	    /// </summary>
	    /// <param name="schedules"></param>
	    IEnumerable<IGroupPage> UserDefinedGroupings(IScheduleDictionary schedules);

	    IBusinessUnit BusinessUnit { get; }

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
