using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// List of values insert event arguments.
    /// </summary>
    /// <remarks>
    /// Created By: kosalanp
    /// Created Date: 02-04-2008
    /// </remarks>
    public class InsertEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for InsertEventArgs.
        /// </summary>
        /// <param name="dataSet">Data source selected by user.</param>
        public InsertEventArgs(IEnumerable<IAggregateRoot> dataSet)
        {
            _dataSet = dataSet;
        }

        /// <summary>
        /// Data source selected property value.
        /// </summary>
        private IEnumerable<IAggregateRoot> _dataSet;
        /// <summary>
        /// Data source selected by user.
        /// </summary>
        public IEnumerable<IAggregateRoot> DataSet { get { return _dataSet; } }
    }
}
