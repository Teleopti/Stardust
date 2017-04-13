using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class DropDownTypeColumn<T, TComboItemType> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly string _headerText;
        private readonly string _groupHeaderText;
        private readonly IList<TComboItemType> _comboItems;

        public event EventHandler<TypeChangeEventArgs> TypeChanged;

        public DropDownTypeColumn(string bindingProperty, string headerText, IList<TComboItemType> comboItems, string groupHeaderText) : base(bindingProperty,130)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _groupHeaderText = groupHeaderText;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.RowIndex == 0 && e.ColIndex > 0)
                {
                    e.Style.CellValue = _headerText;
                }
                else
                {
                    T dataItem = dataItems[e.RowIndex - 1];
					e.Style.CellType = GridCellModelConstants.CellTypeComboBox;
                    e.Style.DataSource = _comboItems;
                    e.Style.DisplayMember = "Name";
                    e.Style.ValueMember = "ClassType";
                    e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                }
            }
            else
            {
                if (e.RowIndex == 0)
                {
                    e.Style.CellValue = _groupHeaderText;
                }
                else if (e.RowIndex == 1 && e.ColIndex > 0)
                {
                    e.Style.CellValue = _headerText;
                }
                else
                {
                    T dataItem = dataItems[e.RowIndex - 2];
                    object cellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
					e.Style.CellType = GridCellModelConstants.CellTypeComboBox;
                    e.Style.BackColor = System.Drawing.Color.Gray;
                    if (cellValue == null)
                    {
                        e.Style.CellType = "Static";
                    }
                    else
                    {
                        e.Style.ResetBackColor();
                        e.Style.DataSource = _comboItems;
                        e.Style.DisplayMember = "Name";
                        e.Style.ValueMember = "ClassType";
                    }
                    e.Style.CellValue = cellValue;
                }
            }


            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                {
                    T dataItem = dataItems[e.RowIndex - 1];

                    if (e.Style.CellValue is Type)
                    {
                        TypeChangeEventArgs args = new TypeChangeEventArgs();
                        args.NewType = ((Type)e.Style.CellValue);
                        args.DataItem = dataItem;
                        OnTypeChanged(args);
                        if (args.IsDataItemValid)
                            OnCellChanged(dataItem);
                    }

                    e.Handled = true;
                }
            }
            else
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                {
                    T dataItem = dataItems[e.RowIndex - 2];

                    if (e.Style.CellValue is Type)
                    {
                        TypeChangeEventArgs args = new TypeChangeEventArgs();
                        args.NewType = ((Type)e.Style.CellValue);
                        args.DataItem = dataItem;
                        OnTypeChanged(args);
                        if (args.IsDataItemValid)
                            OnCellChanged(dataItem);
                    }
                    else if (e.Style.CellValue is String)  // in some paste implementation the clip items are converted to String from System.Type
                    {
                        if (!String.IsNullOrEmpty(e.Style.CellValue.ToString()))
                        {
                            var type = typeof(Person).Assembly.GetType(e.Style.CellValue.ToString());
                            TypeChangeEventArgs args = new TypeChangeEventArgs();
                            args.NewType = type;
                            args.DataItem = dataItem;
                            OnTypeChanged(args);
                            if (args.IsDataItemValid)
                                OnCellChanged(dataItem);
                        }
                    }
                    e.Handled = true;
                }
            }
        }

        public virtual void OnTypeChanged(TypeChangeEventArgs args)
        {
        	var handler = TypeChanged;
        	if (handler != null)
        	{
        		handler(this, args);
        	}
        }
    }
}