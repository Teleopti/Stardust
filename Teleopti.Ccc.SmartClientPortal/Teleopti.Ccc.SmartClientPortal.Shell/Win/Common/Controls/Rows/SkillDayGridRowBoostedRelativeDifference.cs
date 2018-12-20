using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillDayGridRowBoostedRelativeDifference : SkillDayGridRow
    {
        private readonly RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> _rowManager;
        private readonly ISkill _skill;
	    private readonly DailyBoostedSkillForecastAndScheduledValueCalculator _calculator;

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
		    "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	    public SkillDayGridRowBoostedRelativeDifference(
		    RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> rowManager, string cellType,
			    string displayMember, string rowHeaderText, ISkill skill, ISkillPriorityProvider skillPriorityProvider)
		    : base(rowManager, cellType, displayMember, rowHeaderText)
	    {
		    _rowManager = rowManager;
		    _skill = skill;

		    _calculator =
			    new DailyBoostedSkillForecastAndScheduledValueCalculator(
				    () => _rowManager.SchedulerStateHolder.SchedulingResultState, skillPriorityProvider, new SpecificTimeZone(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone));
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

            ForecastScheduleValuePair ret;
            if(_skill.IsVirtual)
            {
                double dailyForecast = 0;
                double tweakedBoostedDailyScheduled = 0;
                foreach (var aggregateSkill in _skill.AggregateSkills)
                {
                    ret = _calculator.CalculateDailyForecastAndScheduleDataForSkill(aggregateSkill, GetDateFromColumn(cellInfo));
                    dailyForecast += ret.ForecastValue;
                    tweakedBoostedDailyScheduled += ret.ScheduleValue;
                }
                if (dailyForecast == 0)
                    return null;
                return (tweakedBoostedDailyScheduled / dailyForecast) * 100;
            }

            ret = _calculator.CalculateDailyForecastAndScheduleDataForSkill(_skill, GetDateFromColumn(cellInfo));
            if (ret.ForecastValue == 0)
                return null;
            return (ret.ScheduleValue/ret.ForecastValue) * 100;
        }
    }
}