﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
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

		public TeamBlockRetryRemoveShiftCategoryBackToLegalService(ITeamBlockScheduler teamBlockScheduler, 
			ITeamInfoFactory teamInfoFactory, 
			ITeamBlockInfoFactory teamBlockInfoFactory, 
			ITeamBlockClearer teamBlockClearer, 
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, 
			IShiftCategoryLimitCounter shiftCategoryLimitCounter, 
			IWorkShiftSelector workShiftSelector, 
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			RemoveScheduleDayProsBasedOnShiftCategoryLimitation removeScheduleDayProsBasedOnShiftCategoryLimitation,
			IScheduleDayChangeCallback scheduleDayChangeCallback)
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
		}

		public void Execute(ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro,
			ISchedulingResultStateHolder schedulingResultStateHolder, 
			IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<IScheduleMatrixPro> scheduleMatrixListPros,
			IOptimizationPreferences optimizationPreferences)
		{
			var shiftNudgeDirective = new ShiftNudgeDirective();

			foreach (var limitation in scheduleMatrixPro.SchedulePeriod.ShiftCategoryLimitationCollection())
			{
				var rollbackService = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

				var unsuccessfulDays = new HashSet<DateOnly>();
				executePerShiftCategoryLimitation(schedulingOptions, scheduleMatrixPro, schedulingResultStateHolder,
					rollbackService, resourceCalculateDelayer, scheduleMatrixListPros, shiftNudgeDirective, optimizationPreferences, limitation, unsuccessfulDays);

				unsuccessfulDays.ForEach(x => scheduleMatrixPro.UnlockPeriod(x.ToDateOnlyPeriod()));
				_removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, scheduleMatrixPro, optimizationPreferences, limitation, rollbackService);
			}
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