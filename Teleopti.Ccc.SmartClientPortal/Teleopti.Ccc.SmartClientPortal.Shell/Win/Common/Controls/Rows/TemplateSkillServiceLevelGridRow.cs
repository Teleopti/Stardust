using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class TemplateSkillServiceLevelGridRow : SkillDataGridRow
    {
        private readonly RowManager<SkillDataGridRow, ISkillData> _rowManager;
        private static readonly TimeSpan MaximumTotalServiceLevelTimeSpan = TimeSpan.FromDays(2);

        public new event EventHandler<FromCellEventArgs<ISkillData>> SaveCellValue;

        public TemplateSkillServiceLevelGridRow(RowManager<SkillDataGridRow, ISkillData> rowManager, string cellType, string displayMember, string rowHeaderText) : base(rowManager, cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
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
            	cellInfo.Style.ReadOnly = true;
            }
            else
            {
                if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0||cellInfo.Style == null) return;

                int rowHeaders = cellInfo.RowHeaderCount;
                if (Math.Max(rowHeaders, cellInfo.ColIndex) - rowHeaders >= _rowManager.Intervals.Count) return;
                ISkillData skillDataPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, rowHeaders);
                int colSpan = GetColSpan(_rowManager, skillDataPeriod.Period);
                if (colSpan > 1)
                {
                    int startCol = GetStartPosition(_rowManager, skillDataPeriod.Period, rowHeaders, ref colSpan);
                    _rowManager.Grid.AddCoveredRange(GridRangeInfo.Cells(cellInfo.RowIndex, startCol, cellInfo.RowIndex, startCol + colSpan - 1));
                }

                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(skillDataPeriod);
                var intervalSpecification = new IntervalLengthServiceLevelSpecification(_rowManager.IntervalLength);
                if(!intervalSpecification.IsSatisfiedBy(skillDataPeriod.ServiceLevelSeconds))
                {
                    cellInfo.Style.Interior = ColorHelper.WarningBrush;
                    cellInfo.Style.CellTipText = UserTexts.Resources.TheServiceLevelForSpecifiedIntervalLengthIsTooLongDot;
                }
            }
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
            int colHeaders = _rowManager.Grid.Cols.HeaderCount + 1;
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0) return;

            ISkillData skillDataPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, colHeaders);
            var cellValue = cellInfo.Style.CellValue is TimeSpan ? (TimeSpan) cellInfo.Style.CellValue : new TimeSpan();
            if (cellValue > MaximumTotalServiceLevelTimeSpan)
               cellInfo.Style.CellValue = MaximumTotalServiceLevelTimeSpan;
            SetValue(skillDataPeriod, cellInfo.Style.CellValue);

        	var handler = SaveCellValue;
            if (handler!= null)
            {
                handler.Invoke(this, new FromCellEventArgs<ISkillData>
                {
                    Item = skillDataPeriod,
                    Style = cellInfo.Style,
                    Value = cellInfo.Style.CellValue
                });
            }
        }
    }
}
