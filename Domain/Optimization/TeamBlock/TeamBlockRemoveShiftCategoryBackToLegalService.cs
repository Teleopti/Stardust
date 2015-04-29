using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockRemoveShiftCategoryBackToLegalService
	{
		bool Execute(IVirtualSchedulePeriod schedulePeriod, ISchedulingOptions schedulingOptions, IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator, IScheduleMatrixPro scheduleMatrixPro, ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> allScheduleMatrixPros, ShiftNudgeDirective shiftNudgeDirective);
	}

	public class TeamBlockRemoveShiftCategoryBackToLegalService : ITeamBlockRemoveShiftCategoryBackToLegalService
	{
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly IShiftCategoryWeekRemover _shiftCategoryWeekRemover;
		private readonly IShiftCategoryPeriodRemover _shiftCategoryPeriodRemover;

		public TeamBlockRemoveShiftCategoryBackToLegalService(ITeamBlockScheduler teamBlockScheduler, ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockClearer teamBlockClearer,  ITeamBlockSchedulingOptions teamBlockSchedulingOptions, IShiftCategoryWeekRemover shiftCategoryWeekRemover, IShiftCategoryPeriodRemover shiftCategoryPeriodRemover)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_shiftCategoryWeekRemover = shiftCategoryWeekRemover;
			_shiftCategoryPeriodRemover = shiftCategoryPeriodRemover;
		}

		public bool Execute(IVirtualSchedulePeriod schedulePeriod, ISchedulingOptions schedulingOptions, IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator, IScheduleMatrixPro scheduleMatrixPro, ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> allScheduleMatrixPros, ShiftNudgeDirective shiftNudgeDirective)
		{
			var success = true;
			var removedScheduleDayPros = new List<IScheduleDayPro>();
			var isSingleAgentTeam = _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions);

			foreach (var limitation in schedulePeriod.ShiftCategoryLimitationCollection())
			{
				if (limitation.Weekly) removedScheduleDayPros.AddRange(_shiftCategoryWeekRemover.Remove(limitation, schedulingOptions, scheduleMatrixValueCalculator, scheduleMatrixPro));
				else removedScheduleDayPros.AddRange(_shiftCategoryPeriodRemover.RemoveShiftCategoryOnPeriod(limitation, schedulingOptions, scheduleMatrixValueCalculator, scheduleMatrixPro));	
				
				foreach (var removedScheduleDayPro in removedScheduleDayPros)
				{
					var dateOnly = removedScheduleDayPro.Day;
					var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
					var teamInfo = _teamInfoFactory.CreateTeamInfo(removedScheduleDayPro.DaySchedulePart().Person, dateOnlyPeriod, allScheduleMatrixPros);
					var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, isSingleAgentTeam);
					if (teamBlockInfo == null) continue;

					success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);
					if (success) continue;

					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
					success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);
					if (success) continue;

					rollbackService.Rollback();
					
					foreach (var selectedBlockDay in teamBlockInfo.BlockInfo.UnLockedDates())
					{
						resourceCalculateDelayer.CalculateIfNeeded(selectedBlockDay, null);
					}
				}
			}

			if(!_teamBlockSchedulingOptions.IsTeamScheduling(schedulingOptions) && !_teamBlockSchedulingOptions.IsTeamBlockScheduling(schedulingOptions)) 
				schedulingOptions.NotAllowedShiftCategories.Clear();

			return success;
		}
	}
}
