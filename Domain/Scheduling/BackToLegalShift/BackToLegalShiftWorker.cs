using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.BackToLegalShift
{
	public interface IBackToLegalShiftWorker
	{
		bool ReSchedule(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
			IShiftProjectionCache roleModelShift,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder, bool isMaxSeatToggleEnabled);
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
			ISchedulingResultStateHolder schedulingResultStateHolder, bool isMaxSeatToggleEnabled)
		{
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
			var date = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;
			bool success = _teamBlockSingleDayScheduler.ScheduleSingleDay(teamBlockInfo, schedulingOptions, date, roleModelShift,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, null, isMaxSeatToggleEnabled);
			if (!success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
			}

			rollbackService.ClearModificationCollection();

			return success;
		}
	}
}