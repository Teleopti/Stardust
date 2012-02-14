#region Imports

using System.ComponentModel;
using Syncfusion.Windows.Forms.Grid;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    /// <summary>
    /// Provides data for a cancelable event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SFGridColumnCellCancelEventArgs<T> : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SFGridColumnCellCancelEventArgs<T>" /> class.
        /// </summary>
        protected SFGridColumnCellCancelEventArgs() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SFGridColumnCellCancelEventArgs<T>" /> class.
        /// </summary>
        /// <param name="dataItem">A bound instance.</param>
        public SFGridColumnCellCancelEventArgs(T dataItem) : this()
        {
            DataItem = dataItem;
        }
        /// <summary>
        /// Gets bound data.
        /// </summary>
        public T DataItem { get; protected set; }
    }
}