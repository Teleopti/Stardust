#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    /// <summary>
    /// Use to consider whether TextOrDropDownColumn column needs to show drop down or the text.
    /// If the method compare returns "true" it will shows the drop down list otherwise it will 
    /// display it as a text column.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 8/27/2008
    /// </remarks>
    public interface ITextOrDropDownColumnComparer<T>
    {
        bool Compare(T dataItem);
    }

    /// <summary>
    /// Represents the  text or drop down column.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 8/27/2008
    /// </remarks>
    public class TextOrDropDownColumn<TData, TItem> : ColumnBase<TData>
    {

        #region Fields - Instance Member

        private string _headerText;
        private string _groupHeaderText;
        private string _bindingProperty;
        private string _displayMember;

        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private IList<TItem> _comboItems;
        //private TypedBindingCollection<TItem> _itemCollection = new TypedBindingCollection<TItem>();
        private ITextOrDropDownColumnComparer<TData> _dropDownVisibleConditionComparer;
        public event EventHandler<SelectedItemChangeEventArgs<TData, TItem>> SelectedItemChanged;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - TextOrDropDownColumn Members

        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
        }

        #endregion

        #endregion

        #region Events - Instance Member

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - TextOrDropDownColumn Members

        #region Methods - Instance Member - TextOrDropDownColumn Members - Constructors

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TextOrDropDownColumn&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="comboItems">The combo items.</param>
        /// <param name="dropDownVisibleConditionComparer">Comparer to chech whether it needs to show the drop down with in the column</param>
        /// <param name="displayMember">The display member.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/27/2008
        /// </remarks>
        public TextOrDropDownColumn(string bindingProperty, string headerText, IList<TItem> comboItems,
            ITextOrDropDownColumnComparer<TData> dropDownVisibleConditionComparer, string displayMember)
        {
            InParameter.NotNull("comboItems", comboItems);
            InParameter.NotNull("dropDownVisibleConditionComparer", dropDownVisibleConditionComparer);

            _headerText = headerText;
            _comboItems = comboItems;
            _bindingProperty = bindingProperty;
            _dropDownVisibleConditionComparer = dropDownVisibleConditionComparer;
            _displayMember = displayMember;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextOrDropDownColumn&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <param name="headerText">The header text.</param>
        /// <param name="comboItems">The combo items.</param>
        /// <param name="groupHeaderText">The group header text.</param>
        /// <param name="dropDownVisibleConditionComparer">
        /// Comparer to chech whether it needs to show the drop down with in the column
        /// </param>
        /// <param name="displayMember">The display member.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/27/2008
        /// </remarks>
        public TextOrDropDownColumn(string bindingProperty, string headerText, IList<TItem> comboItems,
            string groupHeaderText, ITextOrDropDownColumnComparer<TData> dropDownVisibleConditionComparer, string displayMember)
        {
            InParameter.NotNull("comboItems", comboItems);
            InParameter.NotNull("dropDownVisibleConditionComparer", dropDownVisibleConditionComparer);

            _headerText = headerText;
            _comboItems = comboItems;
            _bindingProperty = bindingProperty;
            _groupHeaderText = groupHeaderText;
            _dropDownVisibleConditionComparer = dropDownVisibleConditionComparer;
            _displayMember = displayMember;
        }

        #endregion

        #endregion

        #region Methods - Instance Member - ColumnBase Members

        /// <summary>
        /// Gets the width of the preferred.
        /// </summary>
        /// <value>The width of the preferred.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/27/2008
        /// </remarks>
        public override int PreferredWidth
        {
            get { return 130; }
        }

        /// <summary>
        /// Gets the cell info.
        /// </summary>
        /// <param name="e">The 
        /// <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/27/2008
        /// </remarks>
        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.RowIndex == 0 && e.ColIndex > 0)
                {
                    e.Style.CellValue = _headerText;
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
            }

            if (e.RowIndex > 0 && e.ColIndex > 0)
            {
                TData dataItem;

                if (string.IsNullOrEmpty(_groupHeaderText)) dataItem = dataItems[e.RowIndex - 1];
                else dataItem = dataItems[e.RowIndex - 2];

                object bindingProperty = _propertyReflector.GetValue(dataItem, _bindingProperty);

                if (_dropDownVisibleConditionComparer.Compare(dataItem))
                {
                    e.Style.CellType = "ComboBox";
                    e.Style.DataSource = _comboItems;
                    e.Style.DisplayMember = _displayMember;

                    if (bindingProperty != null)
                    {
                        // Sets the cell value
                        //e.Style.CellValue = bindingProperty;
                        e.Style.CellValue = _propertyReflector.GetValue(bindingProperty, _displayMember);
                    }
                }
                else
                {
                    e.Style.TextColor = Color.DimGray;
                    e.Style.CellType = "Static";

                    if (bindingProperty != null)
                    {
                        // Sets the cell value
                        e.Style.CellValue = _propertyReflector.GetValue(bindingProperty, _displayMember);
                    }
                }

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Saves the cell info.
        /// </summary>
        /// <param name="e">The 
        /// <see cref="Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs"/> 
        /// instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/27/2008
        /// </remarks>
        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<TData> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                {
                    TData dataItem = dataItems[e.RowIndex - 1];

                    if (e.Style.CellValue is TItem)
                    {
                        OnTypeChanged(dataItem, ((TItem)e.Style.CellValue));
                        OnCellChanged(dataItem, e);
                    }

                    // Sets the cell value
                    //e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                    e.Handled = true;
                }
            }
            else
            {
                if (e.ColIndex > 0 && e.RowIndex > 0)
                {
                    TData dataItem = dataItems[e.RowIndex - 2];

                    if (e.Style.CellValue is TItem)
                    {
                        OnTypeChanged(dataItem, ((TItem)e.Style.CellValue));
                        OnCellChanged(dataItem, e);
                    }

                    // Sets the cell value
                    //e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Called when [type changed].
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <param name="selectedItem">The selected item.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/27/2008
        /// </remarks>
        public virtual void OnTypeChanged(TData dataItem, TItem selectedItem)
        {
        	var handler = SelectedItemChanged;
            if (handler!= null)
            {
            	var args = new SelectedItemChangeEventArgs<TData, TItem>(dataItem, selectedItem);
                handler(this, args);
            }
        }

        private object GetCellValue(string cellType, TData dataItem)
        {
            object bindingProperty = _propertyReflector.GetValue(dataItem, _bindingProperty);
            object cellValue = null;

            if (bindingProperty != null)
            {
                if (cellType.Equals("Static"))
                {
                    cellValue = bindingProperty;
                }
                else
                {

                    for (int index = 0; index < _comboItems.Count; index++)
                    {
                        if (bindingProperty.Equals(_comboItems[index]))
                        {
                            cellValue = bindingProperty;// _comboItems[index];
                            break;
                        }
                    }
                }
            }

            return cellValue;
        }

        #endregion

        #endregion

    }

    /// <summary>
    /// Holds the selected item change event argumetns of the drop down column.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-11-18
    /// </remarks>
    public class SelectedItemChangeEventArgs<TData, TItem> : SelectedItemChangeBaseEventArgs<TItem>
    {

        #region Fields - Instance Member

        /// <summary>
        /// Holds the data item.
        /// </summary>
        private TData _dataItem;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - SelectedItemChangeEventArg Members

        /// <summary>
        /// Gets the data item.
        /// </summary>
        /// <value>The data item.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-11-18
        /// </remarks>
        public TData DataItem
        {
            get
            {
                return _dataItem;
            }
        }

        #endregion

        #endregion
        
        #region Methods - Instance Member

        #region Methods - Instance Member - TextOrDropDownColumn Members

        #region Methods - Instance Member - TextOrDropDownColumn Members - Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedItemChangeEventArg&lt;TData, TItem&gt;"/> class.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <param name="selectedItem">The selected item.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-11-18
        /// </remarks>
        public SelectedItemChangeEventArgs(TData dataItem, TItem selectedItem) : base(selectedItem)
        {
            _dataItem = dataItem;
        }

        #endregion

        #endregion

        #endregion
    }


    public class SelectedItemChangeBaseEventArgs<T> : EventArgs
    {

        #region Fields - Instance Member

        /// <summary>
        /// Holds teh selected item of the drop down.
        /// </summary>
        private T _selectedItem;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - SelectedItemChangeEventArgsBase<T> Members

        /// <summary>
        /// Gets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-11-18
        /// </remarks>
        public T SelectedItem
        {
            get
            {
                return _selectedItem;
            }
        }

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - SelectedItemChangeEventArgsBase<T> Members

        #region Methods - Instance Member - SelectedItemChangeEventArgsBase<T> Members - Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedItemChangeEventArgsBase&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="selectedItem">The selected item.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-11-25
        /// </remarks>
        public SelectedItemChangeBaseEventArgs(T selectedItem)
        {
            _selectedItem = selectedItem;
        }

        #endregion

        #endregion

        #endregion

    }
}
