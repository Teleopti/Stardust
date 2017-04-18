using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows
{
    /// <summary>
    /// Class for cell info from grid control
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-23
    /// </remarks>
    public class CellInfo
    {
        /// <summary>
        /// Gets or sets the style.
        /// </summary>
        /// <value>The style.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-23
        /// </remarks>
        public GridStyleInfo Style { get; set; }

        /// <summary>
        /// Gets or sets the index of the col.
        /// </summary>
        /// <value>The index of the col.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-23
        /// </remarks>
        public int ColIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the row.
        /// </summary>
        /// <value>The index of the row.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-23
        /// </remarks>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GridCellInfo"/> is handled.
        /// </summary>
        /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-23
        /// </remarks>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets or sets the col count.
        /// </summary>
        /// <value>The col count.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-23
        /// </remarks>
        public int ColCount { get; set; }

        /// <summary>
        /// Gets or sets the row count.
        /// </summary>
        /// <value>The row count.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-23
        /// </remarks>
        public int RowCount { get; set; }

        /// <summary>
        /// Gets or sets the col header count.
        /// </summary>
        /// <value>The col header count.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-23
        /// </remarks>
        public int ColHeaderCount { get; set; }

        /// <summary>
        /// Gets or sets the row header count.
        /// </summary>
        /// <value>The row header count.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-23
        /// </remarks>
        public int RowHeaderCount { get; set; }
    }
}