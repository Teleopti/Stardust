using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding
{
	public class GridRow<T>
	{
		public GridRow()
		{
			CellValueType = typeof(string);
			HorizontalAlignment = GridHorizontalAlignment.Right;
		}

		public string HeaderText { get; set; }
		public string CellTipText { get; set; }
		public IModelProperty<T> ValueMember { get; set; }
		public Type CellValueType { get; set; }
		public string CellModel { get; set; }
		public object Tag { get; set; }
		public bool ReadOnly { get; set; }
		public GridHorizontalAlignment HorizontalAlignment { get; set; }
	}
}