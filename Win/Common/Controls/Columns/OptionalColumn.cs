using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class OptionalColumn<T> : ColumnBase<T> where T : IEntity, IOptionalColumnView
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly int _maxLength;
        private string _headerText;
        private readonly string _bindingProperty;

        public OptionalColumn(string bindingProperty, int maxLength, string headerText)
        {
            _maxLength = maxLength;
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _columnComparer = new OptionalColumnComparer<T>(_bindingProperty);
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override string BindingProperty
        {
            get
            {
                return _bindingProperty;
            }
        }

        private IComparer<T> _columnComparer;

        /// <summary>
        /// Gets or sets the column comparer.
        /// </summary>
        /// <value>The column comparer.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 8/13/2008
        /// </remarks>
        public override IComparer<T> ColumnComparer
        {
            get { return _columnComparer; }
            set { _columnComparer = value; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Style.MaxLength = _maxLength;
            SetUpSingleHeader(e, dataItems);

            e.Handled = true;
        }

        /// <summary>
        /// Set up single header.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Pubudu Kasakara
        /// Created date: 2008-07-28
        /// </remarks>
        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
                //e.Style.Control.ContextMenuStrip.Items.Clear();
            }
            else
            {
                T dataItem = dataItems[e.RowIndex - 1];
                IList<IOptionalColumn> columns = (IList<IOptionalColumn>)
                    _propertyReflector.GetValue(dataItem, "OptionalColumns");

                var column = columns.FirstOrDefault(c => c.Name == _bindingProperty);
                if (column != null)
                {
                    Guid? id = (Guid?)_propertyReflector.GetValue(dataItem, "Id");
                    IOptionalColumnValue value = column.GetColumnValueById(id);
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

                IList<IOptionalColumn> columns = (IList<IOptionalColumn>)
                _propertyReflector.GetValue(dataItem, "OptionalColumns");
                Guid? id = (Guid?)_propertyReflector.GetValue(dataItem, "Id");

                string _value = e.Style.CellValue.ToString();
                var column = columns.FirstOrDefault(c => c.Name == _bindingProperty);
                if (column != null)
                {
                    IOptionalColumnValue value = column.GetColumnValueById(id);
                    if (value == null)
                    {
                        value = new OptionalColumnValue(_value);
                        value.ReferenceId = id;
                        column.AddOptionalColumnValue(value);
                    }
                    value.Description = _value;
                }
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}