using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    public class EditableAverageWorkTimeInSchedulePeriodColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;

        public EditableAverageWorkTimeInSchedulePeriodColumn(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
        }

        public override int PreferredWidth
        {
            get { return 150; }
        }

		public override string BindingProperty
		{
			get
			{
				return _bindingProperty;
			}
		}

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            SetUpSingleHeader(e, dataItems);
            e.Handled = true;
        }

        /// <summary>
        /// Set up single header.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <param name="dataItems">The data items.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-21
        /// </remarks>
        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
            }

            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                e.Style.CellType = "HourMinutes";
                T dataItem = dataItems[e.RowIndex - 1];
                
                object obj = _propertyReflector.GetValue(dataItem, _bindingProperty);
                TimeSpan timeSpan = (TimeSpan)obj;

                if (timeSpan != TimeSpan.Zero)
                {
                    if(PeopleAdminHelper.IsAverageWorkTimeOverridable(_propertyReflector, dataItem))
                    {
                        e.Style.CellValue = obj;
                    }
                    
                }

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e);
                OnCellDisplayChanged(dataItem, e);
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                T dataItem = dataItems[e.RowIndex - 1];

                if (!string.IsNullOrEmpty(e.Style.CellValue.ToString()))
                {
                    _propertyReflector.SetValue(dataItem, _bindingProperty, (TimeSpan) e.Style.CellValue);

                    if ((TimeSpan) e.Style.CellValue == TimeSpan.Zero)
                    {
                        PeopleAdminHelper.ResetAverageWorkTimePerDay(dataItem);
                    }

                    OnCellChanged(dataItem, e);
                    e.Handled = true;
                }
            }
        }
    }
}