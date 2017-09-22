using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class ValidatedVolumeDayGridRow : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly RowManager<ValidatedVolumeDayGridRow, IValidatedVolumeDay> _rowManager;

        public event EventHandler<FromCellEventArgs<ITaskOwner>> QueryCellValue;

        public ValidatedVolumeDayGridRow(RowManager<ValidatedVolumeDayGridRow, IValidatedVolumeDay> rowManager, string cellType,
                                         string displayMember, string rowHeaderText) 
            : base(cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            if (cellInfo.ColIndex == 0)
            {
                cellInfo.Style.CellValue = RowHeaderText;
            }
            else
            {
                if (_rowManager.DataSource.Count == 0) return;
                if (_rowManager.DataSource.Count < cellInfo.ColIndex) return;
                if (cellInfo.ColIndex == 0) return;
                
                IValidatedVolumeDay validatedVolumeDay = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, cellInfo.RowHeaderCount);
                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(validatedVolumeDay);

            	var handler = QueryCellValue;
                if (handler!= null)
                {
                    handler.Invoke(this, new FromCellEventArgs<ITaskOwner>
                                                    {
                                                        Item = validatedVolumeDay,
                                                        Style = cellInfo.Style
                                                    });
                }
            }
        }

        private object GetValue(IValidatedVolumeDay validatedVolumeDay)
        {
            return _propertyReflector.GetValue(validatedVolumeDay, DisplayMember);
        }

        private void SetValue(IValidatedVolumeDay validatedVolumeDay,object value)
        {
            _propertyReflector.SetValue(validatedVolumeDay, DisplayMember,value);
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
            if (_rowManager.DataSource.Count == 0) return;
            if (cellInfo.ColIndex == 0) return;

            IValidatedVolumeDay validatedVolumeDay = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, cellInfo.RowHeaderCount);
            SetValue(validatedVolumeDay, cellInfo.Style.CellValue);
        }
    }
}