﻿using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Creates projections for a workshiftruleset
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2010-02-15
    /// </remarks>
    public interface IRuleSetProjectionEntityService
    {
	    /// <summary>
	    /// Gets the projection collection.
	    /// </summary>
	    /// <param name="ruleSet">The rule set.</param>
	    /// <param name="callback"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: rogerkr
	    /// Created date: 2010-02-15
	    /// </remarks>
	    IEnumerable<IWorkShiftVisualLayerInfo> ProjectionCollection(IWorkShiftRuleSet ruleSet, IWorkShiftAddCallback callback);
    }
}
