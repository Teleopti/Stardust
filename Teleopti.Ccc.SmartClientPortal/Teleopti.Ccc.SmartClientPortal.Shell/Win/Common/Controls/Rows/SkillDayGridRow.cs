using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillDayGridRow : GridRow
    {
        private readonly RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> _rowManager;
        private IEnumerable<ISkillStaffPeriod> _skillStaffPeriodList;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillDayGridRow(RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> rowManager, string cellType,
                               string displayMember, string rowHeaderText) 
            : base(cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
        }

        protected IEnumerable<ISkillStaffPeriod> SkillStaffPeriodList
        {
            get { return _skillStaffPeriodList; }
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

                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(cellInfo);
                cellInfo.Style.ReadOnly = true;
            }
        }

        protected DateOnly GetDateFromColumn(CellInfo cellInfo)
        {
            DateOnly dateForColumn =
                new DateOnly(
                    _rowManager.BaseDate.AddDays(Math.Max(cellInfo.RowHeaderCount, cellInfo.ColIndex) -
                                                 cellInfo.RowHeaderCount).Date);
            return dateForColumn;
        }

        private IEnumerable<ISkillStaffPeriod> getSkillStaffPeriodsForColumn(CellInfo cellInfo)
        {
            DateTime utcDate = TimeZoneHelper.ConvertToUtc(GetDateFromColumn(cellInfo).Date, _rowManager.TimeZoneInfo);
            IList<ISkillStaffPeriod> skillStaffPeriods;
            if (_rowManager.DataSource[0].TryGetValue(utcDate, out skillStaffPeriods))
                return skillStaffPeriods;
            return new List<ISkillStaffPeriod>();
        }


        protected object GetValue(CellInfo cellInfo)
        {
            _skillStaffPeriodList = getSkillStaffPeriodsForColumn(cellInfo);

			if (DisplayMember == "MaxUsedSeats")
				return SkillStaffPeriodHelper.MaxUsedSeats(SkillStaffPeriodList);

            if(DisplayMember == "ForecastedHours")
                return SkillStaffPeriodHelper.ForecastedTime(SkillStaffPeriodList);

            if(DisplayMember == "ScheduledHours")
                return SkillStaffPeriodHelper.ScheduledTime(SkillStaffPeriodList);

            if (DisplayMember == "AbsoluteDifference")
                return SkillStaffPeriodHelper.AbsoluteDifference(SkillStaffPeriodList);

            if (DisplayMember == "RelativeDifference")
                return SkillStaffPeriodHelper.RelativeDifferenceForDisplay(SkillStaffPeriodList);

            if (DisplayMember == "RootMeanSquare")
                return SkillStaffPeriodHelper.SkillDayRootMeanSquare(SkillStaffPeriodList);

            if (DisplayMember == "DailySmoothness")
                return SkillStaffPeriodHelper.SkillDayGridSmoothness(SkillStaffPeriodList);

            if (DisplayMember == "ForecastedHoursIncoming")
                return SkillStaffPeriodHelper.ForecastedIncoming(SkillStaffPeriodList);

            if (DisplayMember == "ScheduledHoursIncoming")
                return SkillStaffPeriodHelper.ScheduledIncoming(SkillStaffPeriodList);

            if (DisplayMember == "AbsoluteIncomingDifference")
                return SkillStaffPeriodHelper.AbsoluteDifferenceIncoming(SkillStaffPeriodList);

            if (DisplayMember == "RelativeIncomingDifference")
                return SkillStaffPeriodHelper.RelativeDifferenceIncoming(SkillStaffPeriodList);

			if (DisplayMember == "EstimatedServiceLevelShrinkage")
				return SkillStaffPeriodHelper.EstimatedServiceLevelShrinkage(SkillStaffPeriodList);

            return null;
        }

        
    }
}
