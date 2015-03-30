using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Rows
{
    public class SkillDayGridRowBoostedRelativeDifference : SkillDayGridRow
    {
        private readonly RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> _rowManager;
        private readonly ISkill _skill;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillDayGridRowBoostedRelativeDifference(RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> rowManager, string cellType, string displayMember, string rowHeaderText, ISkill skill)
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
            _skill = skill;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
                cellInfo.Style.CellValue = getDoubleValue(cellInfo);
                cellInfo.Style.ReadOnly = true;
            }
        }

        private IEnumerable<ISkillStaffPeriod> getSkillStaffPeriodsForColumn(CellInfo cellInfo)
        {
            DateTime utcDate = TimeZoneHelper.ConvertToUtc(GetDateFromColumn(cellInfo).Date, _rowManager.TimeZoneInfo);
            IList<ISkillStaffPeriod> skillStaffPeriods;
            if (_rowManager.DataSource[0].TryGetValue(utcDate, out skillStaffPeriods))
                return skillStaffPeriods;
            return new List<ISkillStaffPeriod>();
        }

        private double? getDoubleValue(CellInfo cellInfo)
        {
            IEnumerable<ISkillStaffPeriod> skillStaffPeriodList = getSkillStaffPeriodsForColumn(cellInfo);

            if (skillStaffPeriodList == null)
                return null;
            if (!skillStaffPeriodList.Any())
                return null;

            var calculator =
                new DailyBoostedSkillForecastAndScheduledValueCalculator(_rowManager.SchedulerStateHolder.SchedulingResultState);
            ForecastScheduleValuePair ret;
            if(_skill.IsVirtual)
            {
                double dailyForecast = 0;
                double tweakedBoostedDailyScheduled = 0;
                foreach (var aggregateSkill in _skill.AggregateSkills)
                {
                    ret = calculator.CalculateDailyForecastAndScheduleDataForSkill(aggregateSkill, GetDateFromColumn(cellInfo));
                    dailyForecast += ret.ForecastValue;
                    tweakedBoostedDailyScheduled += ret.ScheduleValue;
                }
                if (dailyForecast == 0)
                    return null;
                return (tweakedBoostedDailyScheduled / dailyForecast) * 100;
            }

            ret = calculator.CalculateDailyForecastAndScheduleDataForSkill(_skill, GetDateFromColumn(cellInfo));
            if (ret.ForecastValue == 0)
                return null;
            return (ret.ScheduleValue/ret.ForecastValue) * 100;
        }
    }
}