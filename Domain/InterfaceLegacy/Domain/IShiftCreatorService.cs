using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
	    /// <param name="callback"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: henrika
	    /// Created date: 2008-05-14
	    /// </remarks>
	    IList<WorkShiftCollection> Generate(IWorkShiftRuleSet ruleSet, IWorkShiftAddCallback callback);
    }
}