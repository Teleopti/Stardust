using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillDataGridRow : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly RowManager<SkillDataGridRow, ISkillData> _rowManager;
        
        public event EventHandler<FromCellEventArgs<ISkillData>> SaveCellValue;

        public SkillDataGridRow(RowManager<SkillDataGridRow, ISkillData> rowManager, string cellType, 
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
            }
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
            int colHeaders = _rowManager.Grid.Cols.HeaderCount +1;
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0) return;
            
            ISkillData skillDataPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, colHeaders);
            SetValue(skillDataPeriod, cellInfo.Style.CellValue);

        	var handler = SaveCellValue;
            if (handler != null)
            {
                handler.Invoke(this, new FromCellEventArgs<ISkillData>
                                               {
                                                   Item = skillDataPeriod,
                                                   Style = cellInfo.Style,
                                                   Value = cellInfo.Style.CellValue
                                               });
            }
        }

        protected void SetValue(ISkillData skillDataPeriod, object value)
        {
            _propertyReflector.SetValue(skillDataPeriod, DisplayMember, value);
        }

        protected object GetValue(ISkillData skillDataPeriod)
        {
            return _propertyReflector.GetValue(skillDataPeriod, DisplayMember);
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
        public IList<ISkillData> GetMergeData(int startColumn, int endColumn)
        {
            int colHeaders = _rowManager.Grid.Cols.HeaderCount + 1;
            if (startColumn < colHeaders)
                startColumn = colHeaders;
            ISkillData firstPeriod = GetObjectAtPositionForInterval(_rowManager, startColumn, colHeaders);
            ISkillData lastPeriod = GetObjectAtPositionForInterval(_rowManager, endColumn, colHeaders);

            return (from t in _rowManager.DataSource
                    where t.Period.StartDateTime >= firstPeriod.Period.StartDateTime &&
                          t.Period.StartDateTime <= lastPeriod.Period.StartDateTime
                    orderby t.Period.StartDateTime
                    select t).ToList();
        }
    }
}
