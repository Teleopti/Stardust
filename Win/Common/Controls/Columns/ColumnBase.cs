﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public delegate void Validate<T>(T dataItem, GridStyleInfo styleInfo, int row, bool inSaveMode);

    public abstract class ColumnBase<T> : IColumn<T>, ISortColumn<T>
    {
        public event EventHandler<ColumnCellChangedEventArgs<T>> CellChanged;
        public event EventHandler<ColumnCellDisplayChangedEventArgs<T>> CellDisplayChanged;

        public abstract int PreferredWidth { get; }

        public virtual string BindingProperty { get { return string.Empty; } }

        /// <summary>
        /// Gets or sets the column comparer.
        /// </summary>
        /// <value>The column comparer.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 8/13/2008
        /// </remarks>
        public virtual IComparer<T> ColumnComparer { get; set; }

        public SortCompare<T> SortCompare { get; set; }

        public virtual IComparer<T> Comparer
        {
            get
            {
                IComparer<T> comparer = null;
                if (SortCompare != null)
                    comparer = new ComparerBase<T>(SortCompare);
                return comparer;
            }
        }

        public Validate<T> Validate { get; set; }

        public bool? IsAscending { get; set; }

        public abstract void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems);
        public abstract void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems);

        public bool IsContentRow(int row, int dataItemsCount)
        {
            return row > 0 &&
                   row <= dataItemsCount;
        }

        public virtual void OnCellChanged(T dataItem)
        {
        	var handler = CellChanged;
            if (handler != null)
            {
                ColumnCellChangedEventArgs<T> args = new ColumnCellChangedEventArgs<T>(dataItem);
                handler(this, args);
            }
        }

        public virtual void OnCellChanged(T dataItem, GridSaveCellInfoEventArgs e)
        {
        	var handler = CellChanged;
            if (handler != null)
            {
                ColumnCellChangedEventArgs<T> args = new ColumnCellChangedEventArgs<T>(dataItem, e);
                handler(this, args);
            }
        }

        public virtual void OnCellDisplayChanged(T dataItem, GridQueryCellInfoEventArgs e)
        {
        	var handler = CellDisplayChanged;
            if (handler != null)
            {
                ColumnCellDisplayChangedEventArgs<T> args = new ColumnCellDisplayChangedEventArgs<T>(dataItem, e);
                handler(this, args);
            }
        }

        public virtual void CellsChanging(GridCellsChangingEventArgs e)
        {
        }
    }
}