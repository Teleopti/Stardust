using System;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Rows
{
    public class SkillStaffPeriodGridRowScheduler : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly IRowManager<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;

        public SkillStaffPeriodGridRowScheduler(IRowManager<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> rowManager, string cellType,
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
                if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0) return;
                int rowHeaders = cellInfo.RowHeaderCount;
                if (Math.Max(rowHeaders, cellInfo.ColIndex) - rowHeaders >= _rowManager.Intervals.Count) return;

                ISkillStaffPeriod skillStaffPeriod = GetObjectAtPosition(cellInfo.ColIndex, rowHeaders);
                if (skillStaffPeriod != null)
                {
                    int colSpan = GetColSpan(_rowManager, skillStaffPeriod.Period);
                    if (colSpan > 1)
                    {
                        int startCol = GetStartPosition(_rowManager, skillStaffPeriod.Period, rowHeaders, ref colSpan);
                        _rowManager.Grid.AddCoveredRange(GridRangeInfo.Cells(cellInfo.RowIndex, startCol, cellInfo.RowIndex, startCol + colSpan - 1));
                    }
                    cellInfo.Style.CellType = CellType;
                    cellInfo.Style.CellValue = GetValue(skillStaffPeriod);
                }
                else
                {
                    cellInfo.Style.Text = UserTexts.Resources.Closed;
                    cellInfo.Style.ReadOnly = true;
                }
                cellInfo.Style.Font.FontStyle = GuiSettings.ClosedCellFontStyle();
                cellInfo.Style.TextColor = GuiSettings.ClosedCellFontColor();
            }
        }

        protected ISkillStaffPeriod GetObjectAtPosition(int columnIndex, int rowHeaders)
        {
            DateTime dateTimeToFind = _rowManager.Intervals[columnIndex - rowHeaders].DateTime;
            ISkillStaffPeriod skillStaffPeriod = (from t in _rowManager.DataSource
                                                  where t.Period.StartDateTime <= dateTimeToFind &&
                                                        t.Period.EndDateTime > dateTimeToFind
                                                  orderby t.Period.StartDateTime descending
                                                  select t).FirstOrDefault();

            return skillStaffPeriod;
        }

        private object GetValue(ISkillStaffPeriod skillStaffPeriod)
        {
            return _propertyReflector.GetValue(skillStaffPeriod, DisplayMember);
        }
    }
}