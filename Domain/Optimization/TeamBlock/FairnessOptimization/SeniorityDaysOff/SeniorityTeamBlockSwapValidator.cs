﻿

using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ISeniorityTeamBlockSwapValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences);
	}

	public class SeniorityTeamBlockSwapValidator : ISeniorityTeamBlockSwapValidator
	{
		private readonly IDayOffRulesValidator _dayOffRulesValidator;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _matrixConverter;

		public SeniorityTeamBlockSwapValidator(IDayOffRulesValidator dayOffRulesValidator,
		                                       ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
		                                       IConstructTeamBlock constructTeamBlock, ISchedulingOptionsCreator schedulingOptionsCreator,
			IScheduleMatrixLockableBitArrayConverterEx matrixConverter)
		{
			_dayOffRulesValidator = dayOffRulesValidator;
			_teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_constructTeamBlock = constructTeamBlock;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_matrixConverter = matrixConverter;
		}

		public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences)
		{
			var matrixesToCheck = teamBlockInfo.MatrixesForGroupAndBlock().ToList();

			foreach (var matrix in matrixesToCheck)
			{
				var array = _matrixConverter.Convert(matrix, optimizationPreferences.DaysOff.ConsiderWeekBefore, optimizationPreferences.DaysOff.ConsiderWeekAfter);
				bool valid = _dayOffRulesValidator.Validate(array, optimizationPreferences);
				if (!valid)
					return false;
			}

			var totalPeriod = calculateTotalPeriod(matrixesToCheck);
			var teamBlocksToCheck = _constructTeamBlock.Construct(matrixesToCheck, totalPeriod,
			                                                      teamBlockInfo.TeamInfo.GroupMembers.ToList(),
			                                                      optimizationPreferences.Extra.UseTeamBlockOption,
			                                                      optimizationPreferences.Extra
			                                                                             .BlockFinderTypeForAdvanceOptimization,
			                                                      optimizationPreferences.Extra.GroupPageOnTeamBlockPer);

			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			foreach (var teamBlock in teamBlocksToCheck)
			{
				bool valid = _teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlock, schedulingOptions);
				if (!valid)
					return false;
			}

			return true;
		}

		private static DateOnlyPeriod calculateTotalPeriod(IEnumerable<IScheduleMatrixPro> matrixesToCheck)
		{
			var firstDate = DateOnly.MaxValue;
			var lastDate = DateOnly.MinValue;
			foreach (var matrix in matrixesToCheck)
			{
				var period = matrix.SchedulePeriod.DateOnlyPeriod;
				if (period.StartDate < firstDate)
					firstDate = period.StartDate;

				if (period.EndDate > lastDate)
					lastDate = period.EndDate;
			}
			var totalPeriod = new DateOnlyPeriod(firstDate, lastDate);
			return totalPeriod;
		}
	}
}