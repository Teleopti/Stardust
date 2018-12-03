using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class MultisitePeriodGridRow : GridRow
    {
        private readonly RowManager<MultisitePeriodGridRow, IMultisitePeriod> _rowManager;
        private readonly IChildSkill _childSkill;

        public MultisitePeriodGridRow(RowManager<MultisitePeriodGridRow, IMultisitePeriod> rowManager, string cellType, 
                                      string displayMember, string rowHeaderText, IChildSkill childSkill) 
            : base(cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
            _childSkill = childSkill;
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
                if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count==0) return;
                int rowHeaders = cellInfo.RowHeaderCount;
                if (Math.Max(rowHeaders, cellInfo.ColIndex) - rowHeaders >= _rowManager.Intervals.Count) return;

                IMultisitePeriod multisitePeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, rowHeaders);
                int colSpan = GetColSpan(_rowManager, multisitePeriod.Period);
                if (colSpan > 1)
                {
                    int startCol = GetStartPosition(_rowManager, multisitePeriod.Period, rowHeaders, ref colSpan);
                    _rowManager.Grid.AddCoveredRange(GridRangeInfo.Cells(cellInfo.RowIndex, startCol, cellInfo.RowIndex, startCol + colSpan - 1));
                }
                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(multisitePeriod);

                if (!multisitePeriod.IsValid)
                {
                    cellInfo.Style.Interior = ColorHelper.InvalidDistributionBrush;
                    cellInfo.Style.CellTipText = UserTexts.Resources.InvalidMultisiteDistribution;
                }
            }
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0) return;

            int colHeaders = _rowManager.Grid.Cols.HeaderCount + 1;
            IMultisitePeriod multisitePeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, colHeaders);
            SetValue(multisitePeriod, cellInfo.Style.CellValue);

            //TODO! This should not be here...
            if (multisitePeriod==null) return;
            IMultisiteDay day = multisitePeriod.Parent as IMultisiteDay;
            if (day != null) day.RedistributeChilds();
        }

        private void SetValue(IMultisitePeriod multisitePeriod, object value)
        {
            Percent percent = (Percent)value;
            multisitePeriod.SetPercentage(_childSkill,percent);
        }

        private object GetValue(IMultisitePeriod multisitePeriod)
        {
            if (multisitePeriod.Distribution.ContainsKey(_childSkill))
                return multisitePeriod.Distribution[_childSkill];
            
            return new Percent();
        }

        /// <summary>
        /// Gets the merge data.
        /// </summary>
        /// <param name="startColumn">The start column.</param>
        /// <param name="endColumn">The end column.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        public IList<IMultisitePeriod> GetMergeData(int startColumn, int endColumn)
        {
            int colHeaders = _rowManager.Grid.Cols.HeaderCount + 1;
            if (startColumn < colHeaders)
                startColumn = colHeaders;
            IMultisitePeriod firstPeriod = GetObjectAtPositionForInterval(_rowManager, startColumn, colHeaders);
            IMultisitePeriod lastPeriod = GetObjectAtPositionForInterval(_rowManager, endColumn, colHeaders);

            return (from t in _rowManager.DataSource
                    where t.Period.StartDateTime >= firstPeriod.Period.StartDateTime &&
                          t.Period.StartDateTime <= lastPeriod.Period.StartDateTime
                    orderby t.Period.StartDateTime
                    select t).ToList();
        }
    }
}
