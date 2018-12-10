using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces
{
    /// <summary>
    /// A helper class to wrap up commonly used functions
    /// </summary>
    public interface IPayrollHelper
    {
        /// <summary>
        /// Gets the unit of work.
        /// </summary>
        /// <value>The unit of work.</value>
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Saves the specified definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        void Save(IMultiplicatorDefinitionSet definitionSet);

        /// <summary>
        /// Deletes the specified definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        void Delete(IMultiplicatorDefinitionSet definitionSet);

        /// <summary>
        /// Loads the multiplicators.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        IList<IMultiplicator> LoadMultiplicatorList();

        /// <summary>
        /// Gets all definition sets.
        /// </summary>
        /// <returns></returns>
        IList<IMultiplicatorDefinitionSet> LoadDefinitionSets();
    
    }
}
