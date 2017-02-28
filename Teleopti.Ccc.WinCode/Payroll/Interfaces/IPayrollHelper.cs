using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
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
        /// Loads the multiplicator list.
        /// </summary>
        /// <param name="multiplicatorType">Type of the multiplicator.</param>
        /// <returns></returns>
        IList<IMultiplicator> LoadMultiplicatorList(MultiplicatorType multiplicatorType);

        /// <summary>
        /// Gets all definition sets.
        /// </summary>
        /// <returns></returns>
        IList<IMultiplicatorDefinitionSet> LoadDefinitionSets();
    
    }
}
