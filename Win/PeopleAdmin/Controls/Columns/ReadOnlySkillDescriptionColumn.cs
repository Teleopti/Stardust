using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    class ReadOnlySkillDescriptionColumn<T>:ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;

        public ReadOnlySkillDescriptionColumn(string bindingProperty, string headerText) :base(bindingProperty,100)
        {
            _headerText = headerText;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            
            if (IsContentRow(e.RowIndex,dataItems.Count))
            {
                T dataItem = dataItems.ElementAt(e.RowIndex - 1);

                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);

                PeopleAdminHelper.GrayColumn(_propertyReflector, dataItem, e); 
                
                OnCellDisplayChanged(dataItem, e);
                e.Style.ReadOnly = true;
            }

            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (dataItems.Count == 0) return;
                
                T dataItem = dataItems.ElementAt(e.RowIndex - 1);
                _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.CellValue.ToString());
                OnCellChanged(dataItem, e);
                e.Handled = true;
            }
        }
    }
}
