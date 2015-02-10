using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
	public class SFGridEditableTextColumn<T> : SFGridColumnBase<T>
	{
		private readonly int _maxLength;

		public SFGridEditableTextColumn(string bindingProperty, int maxLength, string headerText)
			: base(bindingProperty, headerText)
		{
			_maxLength = maxLength;
		}

		public override int PreferredWidth
		{
			get { return 100; }
		}

		public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
		{
			e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
			e.Style.MaxLength = _maxLength;
		}

		public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
		{
			var cellValue = e.Style.CellValue as string;
			
			if (AllowEmptyValue)
			{
				if (string.IsNullOrEmpty(cellValue))
					e.Style.CellValue = null;
				else if (cellValue.Length > _maxLength)
					return;
			}
			else
			{
				if (string.IsNullOrWhiteSpace(cellValue) || cellValue.Length > _maxLength)
					return;
			}

			PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
		}
	}
}