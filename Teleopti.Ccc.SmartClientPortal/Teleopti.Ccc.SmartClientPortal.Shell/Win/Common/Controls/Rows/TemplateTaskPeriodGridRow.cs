using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class TemplateTaskPeriodGridRow : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly RowManager<TemplateTaskPeriodGridRow, ITemplateTaskPeriod> _rowManager;
        private readonly int _maxLength;

        public TemplateTaskPeriodGridRow(RowManager<TemplateTaskPeriodGridRow, ITemplateTaskPeriod> rowManager, string cellType, 
                                         string displayMember, string rowHeaderText) 
            : base(cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
        }

        public TemplateTaskPeriodGridRow(RowManager<TemplateTaskPeriodGridRow, ITemplateTaskPeriod> rowManager, string cellType,
                                         string displayMember, string rowHeaderText, int maxLength)
            : this(rowManager, cellType, displayMember, rowHeaderText)
        {
            _maxLength = maxLength;
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            if (cellInfo.Style == null) return;
            if (cellInfo.ColIndex == 0)
            {
                //e.Style.AutoSize = true;
                cellInfo.Style.MergeCell = GridMergeCellDirection.RowsInColumn;
                cellInfo.Style.Font.Orientation = 270;
                cellInfo.Style.VerticalAlignment = GridVerticalAlignment.Middle;
                cellInfo.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                if (cellInfo.RowIndex < 12)
                    cellInfo.Style.CellValue = UserTexts.Resources.Forecasted;
                else
                    cellInfo.Style.CellValue = UserTexts.Resources.Actual;
            }
            else if (cellInfo.ColIndex == 1)
            {
                //e.Style.AutoSize = true;
                cellInfo.Style.CellValue = RowHeaderText;
            }
            
            else
            {
                int rowHeaders = cellInfo.RowHeaderCount;
                if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count==0) return;
                if (Math.Max(rowHeaders, cellInfo.ColIndex) - rowHeaders >= _rowManager.Intervals.Count) return;

                ITemplateTaskPeriod templateTaskPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, rowHeaders);
                if (templateTaskPeriod==null) return;

                int colSpan = GetColSpan(_rowManager, templateTaskPeriod.Period);
                if (colSpan > 1)
                {
                    int startCol = GetStartPosition(_rowManager, templateTaskPeriod.Period, rowHeaders, ref colSpan);
                    _rowManager.Grid.AddCoveredRange(GridRangeInfo.Cells(cellInfo.RowIndex, startCol, cellInfo.RowIndex, startCol + colSpan - 1));
                }
                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = getValue(templateTaskPeriod);
                cellInfo.Style.MaxLength = _maxLength;

                if (((IWorkloadDayBase)templateTaskPeriod.Parent).IsOnlyIncoming(templateTaskPeriod))
                    cellInfo.Style.BackColor = ColorHelper.GridControlIncomingColor();
            }

            GuiHelper.SetStyle(cellInfo.Style);
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
            int colHeaders = _rowManager.Grid.Cols.HeaderCount + 1;
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0) return;
            
            ITemplateTaskPeriod templateTaskPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, colHeaders);
            setValue(templateTaskPeriod, cellInfo.Style.CellValue);
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
        public IList<ITemplateTaskPeriod> GetMergeData(int startColumn, int endColumn)
        {
            int colHeaders = _rowManager.Grid.Cols.HeaderCount +1;
            if (startColumn < colHeaders)
                startColumn = colHeaders;

            if (endColumn == 0)
                endColumn = _rowManager.Grid.ColCount;
            ITemplateTaskPeriod firstTaskPeriod = GetObjectAtPositionForInterval(_rowManager, startColumn, colHeaders);
            ITemplateTaskPeriod lastTaskPeriod = GetObjectAtPositionForInterval(_rowManager, endColumn, colHeaders);

            return (from t in _rowManager.DataSource
                    where t.Period.StartDateTime >= firstTaskPeriod.Period.StartDateTime &&
                          t.Period.StartDateTime <= lastTaskPeriod.Period.StartDateTime
                    orderby t.Period.StartDateTime
                    select t).ToList();
        }

        private void setValue(ITemplateTaskPeriod templateTaskPeriod, object value)
        {
            var type = _propertyReflector.GetType(templateTaskPeriod.GetType(), DisplayMember).UnderlyingSystemType;
            if (String.IsNullOrEmpty(value.ToString()))
            {
                if (type.FullName == "System.Double" || type.FullName == "System.Int32")
                    value = 0;
                else if (type.FullName == "System.TimeSpan")
                    value = TimeSpan.Zero;
                else if (type.FullName == typeof(Percent).FullName)
                    value = new Percent(0);
            }
            _propertyReflector.SetValue(templateTaskPeriod, DisplayMember, value);
        }

        private object getValue(ITemplateTaskPeriod templateTaskPeriod)
        {
            return _propertyReflector.GetValue(templateTaskPeriod, DisplayMember);
        }
    }
}
