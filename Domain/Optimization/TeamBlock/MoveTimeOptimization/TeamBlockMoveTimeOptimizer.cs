using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeOptimizer
	{
		bool OptimizeTeam(IOptimizationPreferences optimizerPreferences, ITeamInfo teamInfo, IScheduleMatrixPro matrix, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IResourceCalculateDelayer resourceCalculateDelayer);
	}

	public class TeamBlockMoveTimeOptimizer : ITeamBlockMoveTimeOptimizer
	{
		private readonly ISchedulingOptionsCreator  _schedulingOptionsCreator;
		private readonly ITeamBlockMoveTimeDescisionMaker _decisionMaker;
		private readonly TeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public TeamBlockMoveTimeOptimizer(ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamBlockMoveTimeDescisionMaker decisionMaker, TeamBlockClearer teamBlockClearer,
			ITeamBlockInfoFactory teamBlockInfoFactory, TeamBlockScheduler teamBlockScheduler,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			IWorkShiftSelector workShiftSelector, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_decisionMaker = decisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public bool OptimizeTeam(IOptimizationPreferences optimizerPreferences, ITeamInfo teamInfo, IScheduleMatrixPro matrix, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			rollbackService.ClearModificationCollection();
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);

			double oldPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			IList<DateOnly> daysToBeMoved = _decisionMaker.Execute(matrix, optimizerPreferences, schedulingResultStateHolder);
			if (daysToBeMoved.Count == 0)
				return false;

			IScheduleDayPro firstDay = matrix.GetScheduleDayByKey(daysToBeMoved[0]);
			DateOnly firstDayDate = daysToBeMoved[0];
			IScheduleDay firstScheduleDay = firstDay.DaySchedulePart();
			TimeSpan firstDayContractTime = firstScheduleDay.ProjectionService().CreateProjection().ContractTime();

			IScheduleDayPro secondDay = matrix.GetScheduleDayByKey(daysToBeMoved[1]);
			DateOnly secondDayDate = daysToBeMoved[1];
			IScheduleDay secondScheduleDay = secondDay.DaySchedulePart();
			TimeSpan secondDayContractTime = secondScheduleDay.ProjectionService().CreateProjection().ContractTime();

			if (firstDayDate == secondDayDate)
				return false;

			if (firstDayContractTime > secondDayContractTime)
			{
				lockDay(teamInfo, secondDayDate);
				return true;
			}

			//delete schedule on the two days
			var firstTeamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, firstDayDate,
				schedulingOptions.BlockFinder());
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, firstTeamBlock);

			var secondTeamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, secondDayDate,
				schedulingOptions.BlockFinder());
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, secondTeamBlock);
																			
			var shiftNudgeDirective = new ShiftNudgeDirective();
			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;
			var resCalcData = new ResourceCalculationData(schedulingResultStateHolder, schedulingOptions.ConsiderShortBreaks, false);
			//TODO: should pass in orginal assignments here to fix same issue as #45540 for shiftswithinday
			var success = _teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), new NoSchedulingCallback(), _workShiftSelector, firstTeamBlock, firstDayDate, schedulingOptions,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.SkillDays, schedulingResultStateHolder.Schedules, resCalcData, shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator);

			if (!success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				return false;
			}

			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;
			//TODO: check assignmentthingy #45540
			success = _teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), new NoSchedulingCallback(), _workShiftSelector, secondTeamBlock, secondDayDate, schedulingOptions,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.SkillDays, schedulingResultStateHolder.Schedules, resCalcData, shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator);

			if (!success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				return false;
			}

			if (!_teamBlockShiftCategoryLimitationValidator.Validate(firstTeamBlock, secondTeamBlock, optimizerPreferences))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				return false;
			}
			
			double newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			bool isPeriodBetter = newPeriodValue < oldPeriodValue;
			if (!isPeriodBetter)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDay(teamInfo, firstDayDate);
				lockDay(teamInfo, secondDayDate);
				return true;
			}

			return true;
		}
	
		private void lockDay(ITeamInfo team, DateOnly day)
		{
			foreach (var matrix in team.MatrixesForGroupAndDate(day))
			{
				matrix.LockDay(day);
			}

		}

	}
}
