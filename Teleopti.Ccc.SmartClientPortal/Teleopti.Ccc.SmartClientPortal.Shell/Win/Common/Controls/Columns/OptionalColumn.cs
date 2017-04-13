using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
	public class OptionalColumn<T> : ColumnBase<T> where T : PersonGeneralModel, IEntity
    {
        private readonly int _maxLength;
        private readonly string _headerText;
        private readonly string _bindingProperty;
		private IComparer<T> _columnComparer;

        public OptionalColumn(string bindingProperty, int maxLength, string headerText) : base(bindingProperty,100)
        {
            _maxLength = maxLength;
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _columnComparer = new OptionalColumnComparer<T>(_bindingProperty);
        }

        public override IComparer<T> ColumnComparer
        {
            get { return _columnComparer; }
            set { _columnComparer = value; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Style.MaxLength = _maxLength;
            setUpSingleHeader(e, dataItems);

            e.Handled = true;
        }

        private void setUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                T dataItem = dataItems[e.RowIndex - 1];
                var column = dataItem.OptionalColumns.FirstOrDefault(c => c.Name == _bindingProperty);
                if (column != null)
                {
                    var value = dataItem.ContainedEntity.GetColumnValue(column);
                    if (value != null)
                    {
                        e.Style.CellValue = value.Description;
                    }
                }
                OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (((string)e.Style.CellValue).Length > _maxLength)
                    return;

                T dataItem = dataItems[e.RowIndex - 1];

				string cellValue = e.Style.CellValue.ToString();
				var column = dataItem.OptionalColumns.FirstOrDefault(c => c.Name == _bindingProperty);
				if (column != null)
				{
					var value = dataItem.ContainedEntity.GetColumnValue(column);
					if (value == null)
					{
						value = new OptionalColumnValue(cellValue);
						dataItem.ContainedEntity.AddOptionalColumnValue(value, column);
					}
					value.Description = cellValue;
				}
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}