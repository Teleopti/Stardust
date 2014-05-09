using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeOptimizer
	{
		bool OptimizeMatrix(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleMatrixPro matrix);
	}

	public class TeamBlockMoveTimeOptimizer : ITeamBlockMoveTimeOptimizer
	{
		private readonly ISchedulingOptionsCreator  _schedulingOptionsCreator;
		private readonly ITeamBlockMoveTimeDescisionMaker _decisionMaker;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ITeamBlockScheduler  _teamBlockScheduler ;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ILockUnSelectedInTeamBlock _lockUnSelectedInTeamBlock;

		public TeamBlockMoveTimeOptimizer(ISchedulingOptionsCreator schedulingOptionsCreator,ITeamBlockMoveTimeDescisionMaker decisionMaker, IDeleteAndResourceCalculateService deleteAndResourceCalculateService, IResourceOptimizationHelper resourceOptimizationHelper,  ITeamBlockScheduler teamBlockScheduler, ITeamBlockGenerator teamBlockGenerator, ILockUnSelectedInTeamBlock lockUnSelectedInTeamBlock)
		{
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_decisionMaker = decisionMaker;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockGenerator = teamBlockGenerator;
			_lockUnSelectedInTeamBlock = lockUnSelectedInTeamBlock;
		}

		public  bool OptimizeMatrix(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleMatrixPro matrix)
		{

			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
			//schedulingOptions.UseCustomTargetTime = _workShiftOriginalStateContainer.OriginalWorkTime();

			double oldPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			IList<DateOnly> daysToBeMoved = _decisionMaker.Execute(matrix ,optimizerPreferences );
			if (daysToBeMoved.Count == 0)
				return false;
			rollbackService.ClearModificationCollection();
			IScheduleDayPro firstDay = matrix.GetScheduleDayByKey(daysToBeMoved[0]);
			DateOnly firstDayDate = daysToBeMoved[0];
			TimeSpan firstDayContractTime = firstDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();

			IScheduleDayPro secondDay = matrix .GetScheduleDayByKey(daysToBeMoved[1]);
			DateOnly secondDayDate = daysToBeMoved[1];
			TimeSpan secondDayContractTime = secondDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();

			if (firstDayDate == secondDayDate)
				return false;

			if (firstDayContractTime > secondDayContractTime)
			{
				lockDay(matrix, secondDayDate);
				return true;
			}
				
			//delete schedule on the two days
			IList<IScheduleDay> listToDelete = new List<IScheduleDay> { firstDay.DaySchedulePart(), secondDay.DaySchedulePart() };
			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(listToDelete, rollbackService, schedulingOptions.ConsiderShortBreaks);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																							schedulingOptions.ConsiderShortBreaks);

			if (!scheduleTeamBlock(matrixList, firstDayDate, matrix, schedulingOptions, rollbackService, resourceCalculateDelayer,schedulingResultStateHolder))
				return false;
			if (!scheduleTeamBlock(matrixList, secondDayDate, matrix, schedulingOptions, rollbackService, resourceCalculateDelayer,schedulingResultStateHolder))
				return false;

		
			double newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			bool isPeriodBetter = newPeriodValue < oldPeriodValue;
			if (!isPeriodBetter)
			{
				return true;
			}

			return true;
		}

		private bool scheduleTeamBlock(IList<IScheduleMatrixPro> matrixList, DateOnly DayDate, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var dayTeamBlock = _teamBlockGenerator.Generate(matrixList, new DateOnlyPeriod(DayDate, DayDate),
				new List<IPerson> { matrix.Person }, schedulingOptions).First();
			_lockUnSelectedInTeamBlock.Lock(dayTeamBlock, new List<IPerson> { matrix.Person }, new DateOnlyPeriod(DayDate, DayDate));
			if (_teamBlockScheduler.ScheduleTeamBlockDay(dayTeamBlock, DayDate, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective()))
			{
				//validate
				return true;
			}
			return false;
		}

		
		private void lockDay(IScheduleMatrixPro matrix, DateOnly day)
		{
			matrix.LockPeriod(new DateOnlyPeriod(day, day));
		}

	}
}
