using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Shifts;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    /// <summary>
    /// Represents the TypeChangeEventArgs
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 2008-10-07
    /// </remarks>
    public class TypeChangeEventArgs : EventArgs
    {
        public Type NewType { get; set; }

        public object DataItem { get; set; }

        public bool IsDataItemValid { get; set; }
    }

    public class DropDownTypeColumn<T, TComboItemType> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly string _headerText;
        private readonly string _groupHeaderText;
        private readonly IList<TComboItemType> _comboItems;
        private readonly string _bindingProperty;

        public event EventHandler<TypeChangeEventArgs> TypeChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropDownTypeColumn&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="comboItems">The combo items.</param>
        public DropDownTypeColumn(string bindingProperty, string headerText, IList<TComboItemType> comboItems)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _bindingProperty = bindingProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DropDownTypeColumn&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="comboItems">The combo items.</param>
        /// <param name="groupHeaderText">The group header text.</param>
        public DropDownTypeColumn(string bindingProperty, string headerText, IList<TComboItemType> comboItems, string groupHeaderText)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _bindingProperty = bindingProperty;
            _groupHeaderText = groupHeaderText;
        }

        #region IColumn<T> Members

        /// <summary>
        /// Gets the width of the preferred.
        /// </summary>
        /// <value>The width of the preferred.</value>
        public override int PreferredWidth
        {
            get { return 130; }
        }

        /// <summary>
        /// Gets the binding property.
        /// </summary>
        /// <value>The binding property.</value>
        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
        }

        /// <summary>
        /// Gets the cell info.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
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
                    e.Style.CellType = "ComboBox";
                    e.Style.DataSource = _comboItems;
                    e.Style.DisplayMember = "Name";
                    e.Style.ValueMember = "ClassType";
                    e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
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
                    object cellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                    e.Style.CellType = "ComboBox";
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

        /// <summary>
        /// Saves the cell info.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
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

     #endregion

        /// <summary>
        /// Called when [type changed].
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <param name="dataItem">The data item.</param>
        public virtual void OnTypeChanged(Type newType, T dataItem)
        {
        	var args = new TypeChangeEventArgs {NewType = newType, DataItem = dataItem};
            OnTypeChanged(args);
        }

        /// <summary>
        /// Called when [type changed].
        /// </summary>
        /// <param name="args">The <see cref="Teleopti.Ccc.Win.Common.Controls.Columns.TypeChangeEventArgs"/> instance containing the event data.</param>
		public virtual void OnTypeChanged(TypeChangeEventArgs args)
        {
        	var handler = TypeChanged;
        	if (handler != null)
        	{
        		handler(this, args);
        	}
        }
    }

    /// <summary>
    /// Represents the DropDownTypeColumn<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 2008-10-07
    /// </remarks>
    public class DropDownTypeColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly string _headerText;
        private readonly string _groupHeaderText;
        private readonly IList<GridClassType> _comboItems;
        private readonly string _bindingProperty;

        public event EventHandler<TypeChangeEventArgs> TypeChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropDownTypeColumn&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="comboItems">The combo items.</param>
        public DropDownTypeColumn(string bindingProperty, string headerText, IList<GridClassType> comboItems)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _bindingProperty = bindingProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DropDownTypeColumn&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="comboItems">The combo items.</param>
        /// <param name="groupHeaderText">The group header text.</param>
        public DropDownTypeColumn(string bindingProperty, string headerText, IList<GridClassType> comboItems, string groupHeaderText)
        {
            _headerText = headerText;
            _comboItems = comboItems;
            _bindingProperty = bindingProperty;
            _groupHeaderText = groupHeaderText;
        }

        #region IColumn<T> Members

        /// <summary>
        /// Gets the width of the preferred.
        /// </summary>
        /// <value>The width of the preferred.</value>
        public override int PreferredWidth
        {
            get { return 130; }
        }

        /// <summary>
        /// Gets the binding property.
        /// </summary>
        /// <value>The binding property.</value>
        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
        }

        /// <summary>
        /// Gets the cell info.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
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
                    e.Style.CellType = "ComboBox";
                    e.Style.DataSource = _comboItems;
                    e.Style.DisplayMember = "Name";
                    e.Style.ValueMember = "ClassType";
                    e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
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
                    object cellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                    e.Style.CellType = "ComboBox";
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

        /// <summary>
        /// Saves the cell info.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
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

                    e.Handled = true;
                }
            }
        }

        #endregion

        /// <summary>
        /// Called when [type changed].
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <param name="dataItem">The data item.</param>
		public virtual void OnTypeChanged(Type newType, T dataItem)
        {
			var args = new TypeChangeEventArgs { NewType = newType, DataItem = dataItem };
        	OnTypeChanged(args);
        }

    	/// <summary>
        /// Called when [type changed].
        /// </summary>
        /// <param name="args">The <see cref="Teleopti.Ccc.Win.Common.Controls.Columns.TypeChangeEventArgs"/> instance containing the event data.</param>
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