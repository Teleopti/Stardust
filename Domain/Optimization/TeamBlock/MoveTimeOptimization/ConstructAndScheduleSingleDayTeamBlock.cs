using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface IConstructAndScheduleSingleDayTeamBlock
	{
		bool Schedule(IList<IScheduleMatrixPro> matrixList, DateOnly dayDate, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, IOptimizationPreferences optimizationPreferences);
	}

	public class ConstructAndScheduleSingleDayTeamBlock : IConstructAndScheduleSingleDayTeamBlock
	{
		private readonly ILockUnSelectedInTeamBlock _lockUnSelectedInTeamBlock;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;

		public ConstructAndScheduleSingleDayTeamBlock(ILockUnSelectedInTeamBlock lockUnSelectedInTeamBlock, ITeamBlockGenerator teamBlockGenerator, ITeamBlockScheduler teamBlockScheduler, ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
		{
			_lockUnSelectedInTeamBlock = lockUnSelectedInTeamBlock;
			_teamBlockGenerator = teamBlockGenerator;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockRestrictionOverLimitValidator = teamBlockRestrictionOverLimitValidator;
		}

		public  bool Schedule(IList<IScheduleMatrixPro> matrixList, DateOnly dayDate, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, IOptimizationPreferences optimizationPreferences)
		{
			var dayTeamBlock = _teamBlockGenerator.Generate(matrixList, new DateOnlyPeriod(dayDate, dayDate),
				new List<IPerson> { matrix.Person }, schedulingOptions).First();
			_lockUnSelectedInTeamBlock.Lock(dayTeamBlock, new List<IPerson> { matrix.Person }, new DateOnlyPeriod(dayDate, dayDate));
			if (_teamBlockScheduler.ScheduleTeamBlockDay(dayTeamBlock, dayDate, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective()))
			{
				if (_teamBlockRestrictionOverLimitValidator.Validate(dayTeamBlock, optimizationPreferences))
				{
					return true;
				}
			}
			
			return false;
		}
	}
}
