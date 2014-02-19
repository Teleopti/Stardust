using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	/// <summary>
    /// Interface for scenario repository
    /// </summary>
    public interface IScenarioRepository : IRepository<IScenario>
	{
        /// <summary>
        /// Finds all scenarios.
        /// Default first, then sorted by name
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-07
        /// </remarks>
        IList<IScenario> FindAllSorted();

        /// <summary>
        /// Finds all scenarios enabled for reporting.
        /// Default first, then sorted by name
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-07
        /// </remarks>
        IList<IScenario> FindEnabledForReportingSorted();

		/// <summary>
		/// Loads the defaut scenario.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Sumeda Herath
		/// Created date: 2008-03-05
		/// </remarks>
		IScenario LoadDefaultScenario();

		IScenario LoadDefaultScenario(IBusinessUnit businessUnit);
    }
}