using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class MultisiteDayGridRow : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly RowManager<MultisiteDayGridRow, IMultisiteDay> _rowManager;
        private readonly IList<DateOnly> _dateTimes;
        private readonly DayHasInvalidMultisiteDistribution _specificationInvalidDistribution = new DayHasInvalidMultisiteDistribution();

        public event EventHandler<FromCellEventArgs<IMultisiteDay>> QueryCellValue;
        public event EventHandler<FromCellEventArgs<IMultisiteDay>> SaveCellValue;
        public event EventHandler<TemplateEventArgs> TemplateSelected;

        public MultisiteDayGridRow(RowManager<MultisiteDayGridRow, IMultisiteDay> rowManager, string cellType, 
                                   string displayMember, string rowHeaderText, IList<DateOnly> dateTimes) 
            : base(cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
            _dateTimes = dateTimes;
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            if (cellInfo.ColIndex == 0)
            {
                VerticalRowHeaderSettings(cellInfo, UserTexts.Resources.Forecasted);
            }
            else if (cellInfo.ColIndex == 1)
            {
                cellInfo.Style.CellValue = RowHeaderText;
            }
            else
            {
                if (_rowManager.DataSource.Count == 0 || _dateTimes.Count==0) return;
                int colHeaders = _rowManager.Grid.Cols.HeaderCount + 1;
                if (Math.Max(colHeaders, cellInfo.ColIndex) - colHeaders >= _rowManager.DataSource.Count) return;
                IMultisiteDay multisiteDay = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, colHeaders);
                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(multisiteDay);

                if (_specificationInvalidDistribution.IsSatisfiedBy(multisiteDay))
                {
                    cellInfo.Style.Interior = ColorHelper.InvalidDistributionBrush;
                    cellInfo.Style.CellTipText = UserTexts.Resources.InvalidMultisiteDistribution;
                }

            	var handler = QueryCellValue;
                if (handler != null)
                {
                    handler.Invoke(this, new FromCellEventArgs<IMultisiteDay> { Item = multisiteDay, Style = cellInfo.Style });
                }
            }
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
            if (_rowManager.DataSource.Count == 0 || _dateTimes.Count == 0) return;
            int colHeaders = _rowManager.Grid.Cols.HeaderCount + 1;
            IMultisiteDay multisiteDay = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, colHeaders);

        	var handler = SaveCellValue;
            if (handler!= null)
            {
                handler.Invoke(this, new FromCellEventArgs<IMultisiteDay>
                                               {
                                                   Item = multisiteDay,
                                                   Style = cellInfo.Style,
                                                   Value = cellInfo.Style.CellValue
                                               });
            }
            else
            {
                SetValue(multisiteDay, cellInfo.Style.CellValue);
            }
        }

        public override void OnSelectionChanged(GridSelectionChangedEventArgs e, int rowHeaders)
        {
            if (TemplateSelected == null) return;
            if (_rowManager.DataSource.Count == 0 || _dateTimes.Count == 0) return;
            IMultisiteDay multisiteDay = GetObjectAtPosition(_rowManager, e.Range.Left, rowHeaders);
            TemplateSelected.Invoke(this, new TemplateEventArgs
                                              {
                                                  TemplateName = multisiteDay.TemplateReference.TemplateName,
                                                  TemplateTarget = TemplateTarget.Multisite
                                              });
        }

        private void SetValue(IMultisiteDay multisiteDay, object value)
        {
            _propertyReflector.SetValue(multisiteDay, DisplayMember, value);
        }

        private object GetValue(IMultisiteDay multisiteDay)
        {
            return _propertyReflector.GetValue(multisiteDay, DisplayMember);
        }
    }
}