using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Win.Common
{

    /// <summary>
    /// Data handling stage throughout working with the form.
    /// </summary>
    public enum DataHandlingStatusOption
    {
        /// <summary>
        /// Not specified
        /// </summary>
        None,
        /// <summary>
        /// Data is loading (Databind data fetch and data move to controls).
        /// </summary>
        DataLoading,
        /// <summary>
        /// User editing data.
        /// </summary>
        DataEditing,
        /// <summary>
        /// Data is saving (data move to datasource and persisting)
        /// </summary>
        DataSaving
    }
}