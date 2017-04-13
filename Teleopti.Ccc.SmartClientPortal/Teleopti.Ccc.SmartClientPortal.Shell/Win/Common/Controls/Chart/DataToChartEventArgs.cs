using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Chart
{
    public class DataToChartEventArgs : EventArgs
    {
        private readonly GridCellModelBase _cellModelType;
        private readonly string _headerText;
        private readonly int _rowIndex;
        private readonly IDictionary<DateTime, double> _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataToChartEventArgs"/> class.
        /// </summary>
        /// <param name="cellModelType">Type of the cell model.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="values">The values.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-12
        /// </remarks>
        public DataToChartEventArgs(GridCellModelBase cellModelType, string headerText, int rowIndex, IDictionary<DateTime, double> values)
        {
            _cellModelType = cellModelType;
            _headerText = headerText;
            _rowIndex = rowIndex;
            _values = values;
        }

        /// <summary>
        /// Gets the type of the cell model.
        /// </summary>
        /// <value>The type of the cell model.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-12
        /// </remarks>
        public GridCellModelBase CellModelType
        {
            get { return _cellModelType; }
        }

        /// <summary>
        /// Gets the header text.
        /// </summary>
        /// <value>The header text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-12
        /// </remarks>
        public string HeaderText
        {
            get { return _headerText; }
        }

        /// <summary>
        /// Gets the index of the row.
        /// </summary>
        /// <value>The index of the row.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-12
        /// </remarks>
        public int RowIndex
        {
            get { return _rowIndex; }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-12
        /// </remarks>
        public IDictionary<DateTime, double> Values
        {
            get { return _values; }
        }
    }
}