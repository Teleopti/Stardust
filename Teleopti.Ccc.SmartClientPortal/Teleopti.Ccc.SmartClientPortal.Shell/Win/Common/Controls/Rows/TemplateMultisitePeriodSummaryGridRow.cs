using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class TemplateMultisitePeriodSummaryGridRow : GridRow
    {
        private readonly RowManager<TemplateMultisitePeriodGridRow, ITemplateMultisitePeriod> _rowManager;
        
        public TemplateMultisitePeriodSummaryGridRow(RowManager<TemplateMultisitePeriodGridRow, ITemplateMultisitePeriod> rowManager, string cellType, 
                                                     string displayMember, string rowHeaderText) 
            : base(cellType, displayMember, rowHeaderText)
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
            }
            else
            {
                if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0) return;
                int rowHeaders = cellInfo.RowHeaderCount;
                ITemplateMultisitePeriod multisitePeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, rowHeaders);
                int colSpan = GetColSpan(_rowManager, multisitePeriod.Period);
                if (colSpan > 1)
                {
                    int startCol = GetStartPosition(_rowManager, multisitePeriod.Period, rowHeaders, ref colSpan);
                    _rowManager.Grid.AddCoveredRange(GridRangeInfo.Cells(cellInfo.RowIndex, startCol, cellInfo.RowIndex, startCol + colSpan - 1));
                }
                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(multisitePeriod);
                cellInfo.Style.ReadOnly = true;

                if (!multisitePeriod.IsValid)
                {
                    cellInfo.Style.Interior = ColorHelper.InvalidDistributionBrush;
                    cellInfo.Style.CellTipText = UserTexts.Resources.InvalidMultisiteDistribution;
                }
            }
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
        }

        private static object GetValue(ITemplateMultisitePeriod multisitePeriod)
        {
            return new Percent(multisitePeriod.Distribution.Values.Sum(d => d.Value));
        }
    }
}
