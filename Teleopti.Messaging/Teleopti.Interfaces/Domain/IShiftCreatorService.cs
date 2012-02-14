using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for ShiftCreatorService
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-05-14
    /// </remarks>
    public interface IShiftCreatorService
    {
        /// <summary>
        /// Generates the workshifts based on ruleset
        /// </summary>
        /// <param name="ruleSet">The rule set.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-05-14
        /// </remarks>
        IList<IWorkShift> Generate(IWorkShiftRuleSet ruleSet);
    }
}