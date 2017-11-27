﻿using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.BackToLegalShift
{
	public interface IBackToLegalShiftWorker
	{
		bool ReSchedule(ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions,
			ShiftProjectionCache roleModelShift,
			ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class BackToLegalShiftWorker : IBackToLegalShiftWorker
	{
		private readonly TeamBlockClearer _teamBlockClearer;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockSingleDayScheduler _teamBlockSingleDayScheduler;
		private readonly IWorkShiftSelector _workShiftSelector;

		public BackToLegalShiftWorker(TeamBlockClearer teamBlockClearer,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamBlockSingleDayScheduler teamBlockSingleDayScheduler,
			IWorkShiftSelector workShiftSelector)
		{
			_teamBlockClearer = teamBlockClearer;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockSingleDayScheduler = teamBlockSingleDayScheduler;
			_workShiftSelector = workShiftSelector;
		}

		public bool ReSchedule(ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions,
			ShiftProjectionCache roleModelShift,
			ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
			var date = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;
			var rules = NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder);
			//TODO: should pass in orginal assignments here to fix same issue as #45540 for shiftswithinday
			var success = _teamBlockSingleDayScheduler.ScheduleSingleDay(Enumerable.Empty<IPersonAssignment>(), _workShiftSelector, teamBlockInfo, schedulingOptions, date, roleModelShift,
				rollbackService, schedulingResultStateHolder.SkillDays, schedulingResultStateHolder.Schedules, new ResourceCalculationData(schedulingResultStateHolder, schedulingOptions.ConsiderShortBreaks, false), null, rules, null);
			if (!success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
			}

			rollbackService.ClearModificationCollection();

			return success;
		}
	}
}