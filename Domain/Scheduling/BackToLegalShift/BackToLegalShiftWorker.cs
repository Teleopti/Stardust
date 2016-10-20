﻿using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
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

		public BackToLegalShiftWorker(ITeamBlockClearer teamBlockClearer,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamBlockSingleDayScheduler teamBlockSingleDayScheduler)
		{
			_teamBlockClearer = teamBlockClearer;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockSingleDayScheduler = teamBlockSingleDayScheduler;
		}

		public bool ReSchedule(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
			IShiftProjectionCache roleModelShift,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
			var date = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;
			var rules = NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder);
			var success = _teamBlockSingleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, date, roleModelShift,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(), null, rules, null);
			if (!success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
			}

			rollbackService.ClearModificationCollection();

			return success;
		}
	}
}