using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
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

		private IEnumerable<ISkillStaffPeriod> getSkillStaffPeriods()
        {
			foreach (var kvp in _rowManager.DataSource[0])
			{
				return kvp.Value;
			}

			return new List<ISkillStaffPeriod>();
        }

		private IEnumerable<IEnumerable<ISkillStaffPeriod>> getSkillStaffPeriodsForFullPeriod()
		{
			var period = _rowManager.DataSource.First().Keys.First();
			var skillStaffPeriodsOfFullPeriod = new List<IList<ISkillStaffPeriod>>();
			foreach (var day in period.DayCollection())
			{
				var dayUtcPeriod = new DateOnlyPeriod(day, day).ToDateTimePeriod(_rowManager.TimeZoneInfo);
				var skillStaffPeriods = SkillStaffPeriodList.Where(x => dayUtcPeriod.Contains(x.Period)).ToList();
				skillStaffPeriodsOfFullPeriod.Add(skillStaffPeriods);
			}
			return skillStaffPeriodsOfFullPeriod;
		}

        protected object GetValue(CellInfo cellInfo)
        {
			if (cellInfo == null) return null;

        	_skillStaffPeriodList = getSkillStaffPeriods(); //(cellInfo);

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
				var skillStaffPeriodOfFullPeriod = getSkillStaffPeriodsForFullPeriod();
				return SkillStaffPeriodHelper.SkillPeriodGridSmoothness(skillStaffPeriodOfFullPeriod);
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
