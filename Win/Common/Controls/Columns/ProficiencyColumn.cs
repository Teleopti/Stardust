using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
	public class ProficiencyColumn<T> : ColumnBase<T>
	{
		private readonly string _bindingPropertySet;
		private readonly string _headerText;
		private readonly PropertyReflector _propertyReflector = new PropertyReflector();
		
		public ProficiencyColumn(string bindingPropertyGet, string bindingPropertySet, string headerText) : base(bindingPropertyGet, 120)
		{
			_bindingPropertySet = bindingPropertySet;
			_headerText = headerText;
		}

		public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
		{
			if (e == null) return;
			if (e.Style == null) return;
			if (e.RowIndex == 0 && e.ColIndex > 0)
			{
				e.Style.CellValue = _headerText;
				e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
			}
			else
			{
				e.Style.BackColor = Color.White;
				e.Style.CellType = "TextBox";
				if (dataItems == null) return;
				T dataItem = dataItems[e.RowIndex - 1];
				var items = _propertyReflector.GetValue(dataItem, BindingProperty) as StringCollection;
				if (items != null && items.Count > 0)
				{
                   e.Style.CellValue = items[0];
					if (items.Count > 1)
						e.Style.BackColor = Color.LightBlue;
				}
				e.Style.DataSource = items;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
		{
			if(e == null) return;
			if(e.Style == null) return;
			if (string.IsNullOrEmpty((string)e.Style.CellValue)) return;
			if (e.ColIndex <= 0 || e.RowIndex < 1) return;
			int intValue;

			try
			{
				intValue = Convert.ToInt32(e.Style.CellValue,CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				return;
			}
			if(dataItems  == null) return;
			T dataItem = dataItems[e.RowIndex - 1];
			_propertyReflector.SetValue(dataItem, _bindingPropertySet, intValue);
			OnCellChanged(dataItem);
		}
	}
}