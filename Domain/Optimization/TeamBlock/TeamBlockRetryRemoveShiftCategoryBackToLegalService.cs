﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockRetryRemoveShiftCategoryBackToLegalService : ITeamBlockRemoveShiftCategoryBackToLegalService
	{
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IShiftCategoryLimitCounter _shiftCategoryLimitCounter;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly RemoveScheduleDayProsBasedOnShiftCategoryLimitation _removeScheduleDayProsBasedOnShiftCategoryLimitation;

		public TeamBlockRetryRemoveShiftCategoryBackToLegalService(ITeamBlockScheduler teamBlockScheduler, 
			ITeamInfoFactory teamInfoFactory, 
			ITeamBlockInfoFactory teamBlockInfoFactory, 
			ITeamBlockClearer teamBlockClearer, 
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions, 
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, 
			IShiftCategoryLimitCounter shiftCategoryLimitCounter, 
			IWorkShiftSelector workShiftSelector, 
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, //TODO: remove! just nu fel pga fel/annorlunda "tag" än förrut
			RemoveScheduleDayProsBasedOnShiftCategoryLimitation removeScheduleDayProsBasedOnShiftCategoryLimitation)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_shiftCategoryLimitCounter = shiftCategoryLimitCounter;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_removeScheduleDayProsBasedOnShiftCategoryLimitation = removeScheduleDayProsBasedOnShiftCategoryLimitation;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> allScheduleMatrixPros,
			ShiftNudgeDirective shiftNudgeDirective, IOptimizationPreferences optimizationPreferences)
		{

			var isSingleAgentTeam = _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions);

			foreach (var limitation in scheduleMatrixPro.SchedulePeriod.ShiftCategoryLimitationCollection())
			{
				var unsuccessfulDays = new HashSet<DateOnly>();
				executePerShiftCategoryLimitation(schedulingOptions, scheduleMatrixPro, schedulingResultStateHolder,
					_schedulePartModifyAndRollbackService, resourceCalculateDelayer, allScheduleMatrixPros, shiftNudgeDirective, optimizationPreferences, limitation, isSingleAgentTeam, unsuccessfulDays);

				unsuccessfulDays.ForEach(x => scheduleMatrixPro.UnlockPeriod(x.ToDateOnlyPeriod()));
				_removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, scheduleMatrixPro, optimizationPreferences, limitation, _schedulePartModifyAndRollbackService);
			}
		}


		private void executePerShiftCategoryLimitation(ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleMatrixPro> allScheduleMatrixPros,
			ShiftNudgeDirective shiftNudgeDirective, IOptimizationPreferences optimizationPreferences,
			IShiftCategoryLimitation limitation, bool isSingleAgentTeam, HashSet<DateOnly> lockedDays)
		{
			rollbackService.ClearModificationCollection(); //TODO: this is maybe wrong - let's see...

			//TODO: ändra så att rollbackservice skickas in hela vägen här
			var removedScheduleDayPros = _removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, scheduleMatrixPro, optimizationPreferences, limitation, _schedulePartModifyAndRollbackService);


			foreach (var removedScheduleDayPro in removedScheduleDayPros)
			{
				var dateOnly = removedScheduleDayPro.Day;
				var teamInfo = _teamInfoFactory.CreateTeamInfo(schedulingResultStateHolder.PersonsInOrganization,
					scheduleMatrixPro.Person, dateOnly.ToDateOnlyPeriod(), allScheduleMatrixPros);
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder(),
					isSingleAgentTeam);
				if (teamBlockInfo == null) continue;

				schedulingOptions.NotAllowedShiftCategories.Clear();

				foreach (var lim in scheduleMatrixPro.SchedulePeriod.ShiftCategoryLimitationCollection())
				{
					var isOnMax = _shiftCategoryLimitCounter.HaveMaxOfShiftCategory(lim, teamInfo, dateOnly);
					if (isOnMax) schedulingOptions.NotAllowedShiftCategories.Add(lim.ShiftCategory);
				}

				var allSkillDays = schedulingResultStateHolder.AllSkillDays();
				var success = _teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, allSkillDays, schedulingResultStateHolder.Schedules, shiftNudgeDirective,
					NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator);
				if (success) continue;

				_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
				success = _teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, allSkillDays, schedulingResultStateHolder.Schedules, shiftNudgeDirective,
					NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator);
				if (success) continue;

				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);

				scheduleMatrixPro.LockPeriod(removedScheduleDayPro.Day.ToDateOnlyPeriod());
				lockedDays.Add(removedScheduleDayPro.Day);

				executePerShiftCategoryLimitation(schedulingOptions, scheduleMatrixPro, schedulingResultStateHolder, rollbackService,
					resourceCalculateDelayer, allScheduleMatrixPros, shiftNudgeDirective, optimizationPreferences, limitation,
					isSingleAgentTeam, lockedDays);
			}
		}
	}
}