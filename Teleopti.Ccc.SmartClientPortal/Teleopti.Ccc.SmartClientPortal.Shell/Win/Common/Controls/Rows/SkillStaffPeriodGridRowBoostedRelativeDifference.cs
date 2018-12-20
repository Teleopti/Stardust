using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillStaffPeriodGridRowBoostedRelativeDifference : SkillStaffPeriodGridRowScheduler
    {
        private readonly RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;
        private readonly ISkill _skill;
	    private readonly DailyBoostedSkillForecastAndScheduledValueCalculator calculator;

	    public SkillStaffPeriodGridRowBoostedRelativeDifference(
		    RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> rowManager, string cellType,
			    string displayMember, string rowHeaderText, ISkill skill, ISkillPriorityProvider skillPriorityProvider)
		    : base(rowManager, cellType, displayMember, rowHeaderText)
	    {
		    _rowManager = rowManager;
		    _skill = skill;
		    calculator =
			    new DailyBoostedSkillForecastAndScheduledValueCalculator(
				    () => _rowManager.SchedulerStateHolder.SchedulingResultState, skillPriorityProvider, new SpecificTimeZone(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone));
	    }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void QueryCellInfo(CellInfo cellInfo)
        {
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0)
            {
                base.QueryCellInfo(cellInfo);
                return;
            }
                
            int rowHeaders = cellInfo.RowHeaderCount;
            if (Math.Max(rowHeaders, cellInfo.ColIndex) - rowHeaders >= _rowManager.Intervals.Count)
            {
                base.QueryCellInfo(cellInfo);
                return;
            }

            if (cellInfo.ColIndex < rowHeaders)
            {
                base.QueryCellInfo(cellInfo);
                return;
            }

            ISkillStaffPeriod skillStaffPeriod = GetObjectAtPosition(cellInfo.ColIndex, rowHeaders);
            if (skillStaffPeriod != null)
            {
                skillStaffPeriod.RelativeBoostedDifferenceForDisplayOnly = calculator.CalculateSkillStaffPeriod(_skill, skillStaffPeriod);
            }
            
            base.QueryCellInfo(cellInfo);
        }
    }
}