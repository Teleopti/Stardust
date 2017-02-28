using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for loading ACD logins
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-18
    /// </remarks>
    public interface IExternalLogOnRepository : IRepository<IExternalLogOn>
    {
	    IList<IExternalLogOn> LoadByAcdLogOnNames(IEnumerable<string> externalLogOnNames);
    }
}