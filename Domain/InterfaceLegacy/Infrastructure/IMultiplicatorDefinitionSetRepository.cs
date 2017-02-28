using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// MultiplicatorDefinitionSet Repository
    /// </summary>
    /// <remarks>
    /// Created by: VirajS
    /// Created date: 2009-01-19
    /// </remarks>
    public interface IMultiplicatorDefinitionSetRepository : IRepository<IMultiplicatorDefinitionSet>
    {
        /// <summary>
        /// Finds all overtime definitions.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-01-17
        /// </remarks>
        IList<IMultiplicatorDefinitionSet> FindAllOvertimeDefinitions();

		/// <summary>
		/// Finds all shift allowance definitions.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: talham
		/// Created date: 2012-09-25
		/// </remarks>
		IList<IMultiplicatorDefinitionSet> FindAllShiftAllowanceDefinitions();

        /// <summary>
        /// Finds all definitions.
        /// </summary>
        /// <returns></returns>
        IList<IMultiplicatorDefinitionSet> FindAllDefinitions();
    }
}
