﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.BackToLegalShift
{
	public interface IBackToLegalShiftWorker
	{
		bool ReSchedule(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
			IShiftProjectionCache roleModelShift,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class BackToLegalShiftWorker : IBackToLegalShiftWorker
	{
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockSingleDayScheduler _teamBlockSingleDayScheduler;
		private readonly IWorkShiftSelector _workShiftSelector;

		public BackToLegalShiftWorker(ITeamBlockClearer teamBlockClearer,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamBlockSingleDayScheduler teamBlockSingleDayScheduler,
			IWorkShiftSelector workShiftSelector)
		{
			_teamBlockClearer = teamBlockClearer;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockSingleDayScheduler = teamBlockSingleDayScheduler;
			_workShiftSelector = workShiftSelector;
		}

		public bool ReSchedule(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
			IShiftProjectionCache roleModelShift,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
			var date = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;
			var rules = NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder);
			var success = _teamBlockSingleDayScheduler.ScheduleSingleDay(_workShiftSelector, teamBlockInfo, schedulingOptions, date, roleModelShift,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(), schedulingResultStateHolder.Schedules, null, rules, null);
			if (!success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
			}

			rollbackService.ClearModificationCollection();

			return success;
		}
	}
}