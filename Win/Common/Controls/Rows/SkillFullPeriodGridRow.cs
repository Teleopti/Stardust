﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Rows
{
    public class SkillFullPeriodGridRow : GridRow
    {
        private readonly RowManagerScheduler<SkillFullPeriodGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> _rowManager;
		private IEnumerable<ISkillStaffPeriod> _skillStaffPeriodList;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillFullPeriodGridRow(RowManagerScheduler<SkillFullPeriodGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> rowManager, string cellType,
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
			if(cellInfo == null) throw new ArgumentNullException("cellInfo");
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

		
		protected DateOnlyPeriod GetDateOnlyPeriodFromColum(CellInfo cellInfo)
		{
			if(cellInfo == null) throw new ArgumentNullException("cellInfo");
			//var dateForColumn = new DateOnly(_rowManager.BaseDate.AddDays(Math.Max(cellInfo.RowHeaderCount, cellInfo.ColIndex * 7) - cellInfo.RowHeaderCount).Date);

			var dateForColumn = _rowManager.BaseDate.AddMonths(Math.Max(cellInfo.RowHeaderCount, cellInfo.ColIndex) - cellInfo.RowHeaderCount).Date;

			var startDate = DateHelper.GetFirstDateInMonth(dateForColumn.Date, CultureInfo.CurrentCulture);
			var endDate = DateHelper.GetLastDateInMonth(dateForColumn.Date, CultureInfo.CurrentCulture);
			var monthPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			return monthPeriod;
		}

		private IEnumerable<ISkillStaffPeriod> getSkillStaffPeriods()
        {
			//DateOnlyPeriod period = GetDateOnlyPeriodFromColum(cellInfo);
			//IList<ISkillStaffPeriod> skillStaffPeriods;
			//if (_rowManager.DataSource[0].TryGetValue(period, out skillStaffPeriods))
			//    return skillStaffPeriods;

			foreach (var kvp in _rowManager.DataSource[0])
			{
				return kvp.Value;
			}

			return new List<ISkillStaffPeriod>();
        }


        protected object GetValue(CellInfo cellInfo)
        {
			if (cellInfo == null) return null;

        	_skillStaffPeriodList = getSkillStaffPeriods(); //(cellInfo);

			//if (DisplayMember == "MaxUsedSeats")
			//    return SkillStaffPeriodHelper.MaxUsedSeats(SkillStaffPeriodList);

			if (DisplayMember == "ForecastedHours")
				return SkillStaffPeriodHelper.ForecastedTime(SkillStaffPeriodList);

			if (DisplayMember == "ScheduledHours")
				return SkillStaffPeriodHelper.ScheduledTime(SkillStaffPeriodList);

			if (DisplayMember == "AbsoluteDifference")
				return SkillStaffPeriodHelper.AbsoluteDifference(SkillStaffPeriodList, false, false);

			if (DisplayMember == "RelativeDifference")
				return SkillStaffPeriodHelper.RelativeDifferenceForDisplay(SkillStaffPeriodList);

			//if (DisplayMember == "RootMeanSquare")
			//    return SkillStaffPeriodHelper.SkillDayRootMeanSquare(SkillStaffPeriodList);

			if (DisplayMember == "DailySmoothness")
				return SkillStaffPeriodHelper.SkillDayGridSmoothness(SkillStaffPeriodList);

			//if (DisplayMember == "HighestDeviationInPeriod")
			//    return SkillStaffPeriodHelper.GetHighestIntraIntervalDeviation(SkillStaffPeriodList);

			if (DisplayMember == "ForecastedHoursIncoming")
				return SkillStaffPeriodHelper.ForecastedIncoming(SkillStaffPeriodList);

			if (DisplayMember == "ScheduledHoursIncoming")
				return SkillStaffPeriodHelper.ScheduledIncoming(SkillStaffPeriodList);

			if (DisplayMember == "AbsoluteIncomingDifference")
				return SkillStaffPeriodHelper.AbsoluteDifferenceIncoming(SkillStaffPeriodList);

			if (DisplayMember == "RelativeIncomingDifference")
				return SkillStaffPeriodHelper.RelativeDifferenceIncoming(SkillStaffPeriodList);

			if (DisplayMember == "EstimatedServiceLevel")
				return SkillStaffPeriodHelper.EstimatedServiceLevel(SkillStaffPeriodList);

            return null;
        }

        
    }
}
