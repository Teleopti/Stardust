using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Rows
{
    public class SkillStaffPeriodGridRow : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly RowManager<SkillStaffPeriodGridRow, ISkillStaffPeriod> _rowManager;

        public SkillStaffPeriodGridRow(RowManager<SkillStaffPeriodGridRow, ISkillStaffPeriod> rowManager, string cellType,
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
                if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0) return;
                int rowHeaders = cellInfo.RowHeaderCount;
                if (Math.Max(rowHeaders, cellInfo.ColIndex) - rowHeaders >= _rowManager.Intervals.Count) return;
                ISkillStaffPeriod skillStaffPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, rowHeaders);
                if (skillStaffPeriod != null)
                {
                    cellInfo.Style.CellType = CellType;
                    cellInfo.Style.CellValue = GetValue(skillStaffPeriod);
                }
                else
                {
                    cellInfo.Style.Text = UserTexts.Resources.NA;
                    cellInfo.Style.ReadOnly = true;
                }
                cellInfo.Style.Font.FontStyle = GuiSettings.ClosedCellFontStyle();
                cellInfo.Style.TextColor = GuiSettings.ClosedCellFontColor();
            }
        }

        protected override bool AllowMerge
        {
            get
            {
                return false;
            }
        }

        private object GetValue(ISkillStaffPeriod skillStaffPeriod)
        {
            return _propertyReflector.GetValue(skillStaffPeriod, DisplayMember);
        }
    }
}
