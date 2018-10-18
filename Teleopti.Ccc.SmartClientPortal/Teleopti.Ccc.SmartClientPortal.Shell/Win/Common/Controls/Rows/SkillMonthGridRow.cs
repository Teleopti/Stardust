﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillMonthGridRow : GridRow
    {
        private readonly RowManagerScheduler<SkillMonthGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> _rowManager;
		private IEnumerable<ISkillStaffPeriod> _skillStaffPeriodList;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillMonthGridRow(RowManagerScheduler<SkillMonthGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> rowManager, string cellType,
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

		private IEnumerable<ISkillStaffPeriod> getSkillStaffPeriodsForColumn(CellInfo cellInfo)
        {
			DateOnlyPeriod period = GetDateOnlyPeriodFromColum(cellInfo);
			IList<ISkillStaffPeriod> skillStaffPeriods;
			if (_rowManager.DataSource[0].TryGetValue(period, out skillStaffPeriods))
				return skillStaffPeriods;
			return new List<ISkillStaffPeriod>();
        }

		private IEnumerable<IEnumerable<ISkillStaffPeriod>> getSkillStaffPeriodsForOneMonth(CellInfo cellInfo)
		{
			var period = GetDateOnlyPeriodFromColum(cellInfo);
			var skillStaffPeriodsOfOneMonth = new List<IList<ISkillStaffPeriod>>();
			foreach (var day in period.DayCollection())
			{
				var dayUtcPeriod = new DateOnlyPeriod(day, day).ToDateTimePeriod(_rowManager.TimeZoneInfo);
				var skillStaffPeriods = SkillStaffPeriodList.Where(x => dayUtcPeriod.Contains(x.Period)).ToList();
				skillStaffPeriodsOfOneMonth.Add(skillStaffPeriods);
			}
			return skillStaffPeriodsOfOneMonth;
		}

        protected object GetValue(CellInfo cellInfo)
        {
            _skillStaffPeriodList = getSkillStaffPeriodsForColumn	(cellInfo);

			if (DisplayMember == "ForecastedHours")
				return SkillStaffPeriodHelper.ForecastedTime(SkillStaffPeriodList);

			if (DisplayMember == "ScheduledHours")
				return SkillStaffPeriodHelper.ScheduledTime(SkillStaffPeriodList);

			if (DisplayMember == "AbsoluteDifference")
				return SkillStaffPeriodHelper.AbsoluteDifference(SkillStaffPeriodList);

			if (DisplayMember == "RelativeDifference")
				return SkillStaffPeriodHelper.RelativeDifferenceForDisplay(SkillStaffPeriodList);

			if (DisplayMember == "DailySmoothness")
			{
				var skillStaffPeriodOfOneMonth = getSkillStaffPeriodsForOneMonth(cellInfo);
				return SkillStaffPeriodHelper.SkillPeriodGridSmoothness(skillStaffPeriodOfOneMonth);
			}

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
