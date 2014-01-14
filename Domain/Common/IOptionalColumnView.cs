using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{

    /// <summary>
    /// Defines the functionality of a IOptionalColumn
    /// </summary>
    public interface IOptionalColumnView
    {
        /// <summary>
        /// Gets or sets the OptionalColumns
        /// </summary>
        /// <value>OptionalColumns</value>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        IList<IOptionalColumn> OptionalColumns{ get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns"></param>
        void SetOptionalColumns(IList<IOptionalColumn> columns);
    }

}
