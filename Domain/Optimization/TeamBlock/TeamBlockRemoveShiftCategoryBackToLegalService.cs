using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockRemoveShiftCategoryBackToLegalService
	{
		void Execute(ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> allScheduleMatrixPros, ShiftNudgeDirective shiftNudgeDirective, IOptimizationPreferences optimizationPreferences);
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
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IShiftCategoryLimitCounter _shiftCategoryLimitCounter;

		public TeamBlockRemoveShiftCategoryBackToLegalService(ITeamBlockScheduler teamBlockScheduler, ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockClearer teamBlockClearer,  ITeamBlockSchedulingOptions teamBlockSchedulingOptions, IShiftCategoryWeekRemover shiftCategoryWeekRemover, IShiftCategoryPeriodRemover shiftCategoryPeriodRemover, ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, IShiftCategoryLimitCounter shiftCategoryLimitCounter)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_shiftCategoryWeekRemover = shiftCategoryWeekRemover;
			_shiftCategoryPeriodRemover = shiftCategoryPeriodRemover;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_shiftCategoryLimitCounter = shiftCategoryLimitCounter;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> allScheduleMatrixPros, ShiftNudgeDirective shiftNudgeDirective, IOptimizationPreferences optimizationPreferences)
		{
			var removedScheduleDayPros = new List<IScheduleDayPro>();
			var isSingleAgentTeam = _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions);
			var schedulePeriod = scheduleMatrixPro.SchedulePeriod;
			var person = scheduleMatrixPro.Person;
			var used = new List<IShiftCategory>();
			
			foreach (var limitation in schedulePeriod.ShiftCategoryLimitationCollection())
			{
				used.Add(limitation.ShiftCategory);
				removedScheduleDayPros.Clear();

				if (limitation.Weekly) removedScheduleDayPros.AddRange(_shiftCategoryWeekRemover.Remove(limitation, schedulingOptions, scheduleMatrixPro, optimizationPreferences));
				else removedScheduleDayPros.AddRange(_shiftCategoryPeriodRemover.RemoveShiftCategoryOnPeriod(limitation, schedulingOptions, scheduleMatrixPro, optimizationPreferences));

				foreach (var removedScheduleDayPro in removedScheduleDayPros)
				{
					rollbackService.ClearModificationCollection();

					if (removedScheduleDayPro.DaySchedulePart().IsScheduled()) continue;

					var dateOnly = removedScheduleDayPro.Day;
					var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
					var teamInfo = _teamInfoFactory.CreateTeamInfo(person, dateOnlyPeriod, allScheduleMatrixPros);
					var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, isSingleAgentTeam);
					if (teamBlockInfo == null) continue;

					schedulingOptions.NotAllowedShiftCategories.Clear();

					foreach (var lim in schedulePeriod.ShiftCategoryLimitationCollection())
					{
						var isOnMax = _shiftCategoryLimitCounter.HaveMaxOfShiftCategory(limitation, teamInfo, dateOnly);
						if (isOnMax) schedulingOptions.NotAllowedShiftCategories.Add(lim.ShiftCategory);			
					}

					foreach (var shiftCategory in used)
					{	
						schedulingOptions.NotAllowedShiftCategories.Add(shiftCategory);
					}

					var success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);
					if (success) continue;

					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
					success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);
					if (success) continue;

					_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				}
			}
		}
	}
}
