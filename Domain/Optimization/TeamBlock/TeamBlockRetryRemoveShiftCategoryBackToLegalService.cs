using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockRetryRemoveShiftCategoryBackToLegalService
	{
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IShiftCategoryLimitCounter _shiftCategoryLimitCounter;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly RemoveScheduleDayProsBasedOnShiftCategoryLimitation _removeScheduleDayProsBasedOnShiftCategoryLimitation;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IUserTimeZone _userTimeZone;

		public TeamBlockRetryRemoveShiftCategoryBackToLegalService(ITeamBlockScheduler teamBlockScheduler, 
			ITeamInfoFactory teamInfoFactory, 
			ITeamBlockInfoFactory teamBlockInfoFactory, 
			ITeamBlockClearer teamBlockClearer, 
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, 
			IShiftCategoryLimitCounter shiftCategoryLimitCounter, 
			IWorkShiftSelector workShiftSelector, 
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			RemoveScheduleDayProsBasedOnShiftCategoryLimitation removeScheduleDayProsBasedOnShiftCategoryLimitation,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IResourceCalculation resourceCalculation,
			IUserTimeZone userTimeZone)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockClearer = teamBlockClearer;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_shiftCategoryLimitCounter = shiftCategoryLimitCounter;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_removeScheduleDayProsBasedOnShiftCategoryLimitation = removeScheduleDayProsBasedOnShiftCategoryLimitation;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_resourceCalculation = resourceCalculation;
			_userTimeZone = userTimeZone;
		}

		public void Execute(ISchedulingOptions schedulingOptions, 
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IEnumerable<IScheduleMatrixPro> scheduleMatrixListPros,
			IOptimizationPreferences optimizationPreferences,
			ISchedulingProgress backgroundWorker)
		{
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, 1, schedulingOptions.ConsiderShortBreaks, schedulingResultStateHolder, _userTimeZone);
			foreach (var matrix in scheduleMatrixListPros)
			{
				backgroundWorker.ReportProgress(0, new TeleoptiProgressChangeMessage(Resources.TryingToResolveShiftCategoryLimitationsDotDotDot));
				var shiftNudgeDirective = new ShiftNudgeDirective();

				foreach (var limitation in matrix.SchedulePeriod.ShiftCategoryLimitationCollection())
				{
					var rollbackService = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

					var unsuccessfulDays = new HashSet<DateOnly>();
					executePerShiftCategoryLimitation(schedulingOptions, matrix, schedulingResultStateHolder,
						rollbackService, resourceCalculateDelayer, scheduleMatrixListPros, shiftNudgeDirective, optimizationPreferences, limitation, unsuccessfulDays);

					unsuccessfulDays.ForEach(x => matrix.UnlockPeriod(x.ToDateOnlyPeriod()));
					_removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, matrix, optimizationPreferences, limitation, rollbackService);
				}
			}

			//maybe not necessary when we put schedules "correct" above
			var rollbackServiceTemp = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			foreach (var scheduleMatrixPro in scheduleMatrixListPros)
			{
				foreach (var limitation in scheduleMatrixPro.SchedulePeriod.ShiftCategoryLimitationCollection())
				{
					_removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, scheduleMatrixPro, optimizationPreferences, limitation, rollbackServiceTemp);
				}
			}
			//
		}

		private void executePerShiftCategoryLimitation(ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<IScheduleMatrixPro> allScheduleMatrixPros,
			ShiftNudgeDirective shiftNudgeDirective, IOptimizationPreferences optimizationPreferences,
			IShiftCategoryLimitation limitation, HashSet<DateOnly> lockedDays)
		{
			var removedScheduleDayPros = _removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, scheduleMatrixPro, optimizationPreferences, limitation, rollbackService);

			foreach (var removedScheduleDayPro in removedScheduleDayPros)
			{
				var dateOnly = removedScheduleDayPro.Day;
				var teamInfo = _teamInfoFactory.CreateTeamInfo(schedulingResultStateHolder.PersonsInOrganization,
					scheduleMatrixPro.Person, dateOnly.ToDateOnlyPeriod(), allScheduleMatrixPros);
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());
				if (teamBlockInfo == null)
					continue;

				schedulingOptions.NotAllowedShiftCategories.Clear();


				foreach (var matrixPro in teamBlockInfo.MatrixesForGroupAndBlock())
				{
					foreach (var shiftCategoryLimitation in matrixPro.SchedulePeriod.ShiftCategoryLimitationCollection())
					{
						if (_shiftCategoryLimitCounter.HaveMaxOfShiftCategory(shiftCategoryLimitation, teamInfo, dateOnly))
						{
							schedulingOptions.NotAllowedShiftCategories.Add(shiftCategoryLimitation.ShiftCategory);
						}
					}
				}

				var allSkillDays = schedulingResultStateHolder.AllSkillDays();
				if(_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, allSkillDays, schedulingResultStateHolder.Schedules, shiftNudgeDirective,
					NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
					continue;

				_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
				if (_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, allSkillDays, schedulingResultStateHolder.Schedules, shiftNudgeDirective,
					NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
					continue;

				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);

				scheduleMatrixPro.LockDay(removedScheduleDayPro.Day);
				lockedDays.Add(removedScheduleDayPro.Day);

				executePerShiftCategoryLimitation(schedulingOptions, scheduleMatrixPro, schedulingResultStateHolder, rollbackService,
					resourceCalculateDelayer, allScheduleMatrixPros, shiftNudgeDirective, optimizationPreferences, limitation, lockedDays);
			}
		}
	}
}