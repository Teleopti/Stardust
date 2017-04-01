using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Rule set bag repository
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-12-08
    /// </remarks>
    public interface IRuleSetBagRepository : IRepository<IRuleSetBag>
    {
        IEnumerable<IRuleSetBag> LoadAllWithRuleSets();
	    IRuleSetBag FindWithRuleSetsAndAccessibility(Guid id);
		IRuleSetBag[] FindWithRuleSetsAndAccessibility(Guid[] ruleBagIds);
    }
}