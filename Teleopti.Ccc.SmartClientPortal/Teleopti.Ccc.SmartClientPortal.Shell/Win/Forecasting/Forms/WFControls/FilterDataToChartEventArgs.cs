using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls
{
	public class FilterDataToChartEventArgs : EventArgs
	{
		private readonly string _headerText;
		private readonly int _columnIndex;
		private readonly IDictionary<DateTime, double> _values;
		private readonly bool _removeColumn;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
		public FilterDataToChartEventArgs(string headerText, int columnIndex, IDictionary<DateTime, double> values, bool removeColumn = false)
		{
			_headerText = headerText;
			_columnIndex = columnIndex;
			_values = values;
			_removeColumn = removeColumn;
		}

		public bool RemoveColumn
		{
			get { return _removeColumn; }
		}

		public IDictionary<DateTime, double> Values
		{
			get { return _values; }
		}

		public string HeaderText
		{
			get { return _headerText; }
		}

		public int ColumnIndex
		{
			get { return _columnIndex; }
		}
	}
}
