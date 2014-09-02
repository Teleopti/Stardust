﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeBetweenDaysService
	{
		void Execute(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList,
			ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator,
			ISchedulingResultStateHolder schedulingResultStateHolder, IList<IPerson> selectedPersons,
			DateOnlyPeriod selectedPeriod);
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockMoveTimeBetweenDaysService : ITeamBlockMoveTimeBetweenDaysService
	{
		private readonly ITeamBlockMoveTimeOptimizer _teamBlockMoveTimeOptimizer;
		private bool _cancelMe;
		private readonly IConstructTeamBlock  _constructTeamBlock;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public TeamBlockMoveTimeBetweenDaysService(ITeamBlockMoveTimeOptimizer teamBlockMoveTimeOptimizer, IConstructTeamBlock constructTeamBlock, IFilterForTeamBlockInSelection filterForTeamBlockInSelection, IFilterForNoneLockedTeamBlocks filterForNoneLockedTeamBlocks)
		{
			_teamBlockMoveTimeOptimizer = teamBlockMoveTimeOptimizer;
			_constructTeamBlock = constructTeamBlock;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_filterForNoneLockedTeamBlocks = filterForNoneLockedTeamBlocks;
		}

		public void Execute(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList,
			ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator,
			ISchedulingResultStateHolder schedulingResultStateHolder, IList<IPerson> selectedPersons,
			DateOnlyPeriod selectedPeriod)
		{
			var blocksToWorkWith = _constructTeamBlock.Construct(matrixList, selectedPeriod, selectedPersons,
				optimizerPreferences.Extra.BlockTypeValue,
				optimizerPreferences.Extra.TeamGroupPage);


			blocksToWorkWith = _filterForTeamBlockInSelection.Filter(blocksToWorkWith,
				selectedPersons, selectedPeriod);

			blocksToWorkWith = _filterForNoneLockedTeamBlocks.Filter(blocksToWorkWith);
			var teamsToWorkWith = blocksToWorkWith.Select(s => s.TeamInfo).ToList();

			_cancelMe = false;
			var activeTeams = new List<ITeamInfo>(teamsToWorkWith);
			while (activeTeams.Any())
			{
				var team = activeTeams.GetRandom(activeTeams.Count(), true).FirstOrDefault();
				if (team == null) break;
				var selectedMatrixes = team.MatrixesForUnlockedMembersAndPeriod(selectedPeriod).ToList();
				if (_cancelMe)
					break;
				IEnumerable<IScheduleMatrixPro> shuffledMatrixes =selectedMatrixes.GetRandom(selectedMatrixes.Count, true);
				int executes = 0;
				foreach (var matrix in shuffledMatrixes)
				{
					if (_cancelMe)
						break;
					executes++;
					bool result = _teamBlockMoveTimeOptimizer.OptimizeMatrix(optimizerPreferences, matrixList, rollbackService,
						periodValueCalculator, schedulingResultStateHolder, matrix, selectedMatrixes);
					if (!result)
					{
						//	continue;
						activeTeams.Remove(team);
					}
					double newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
					if (_cancelMe)
						break;
					string who = Resources.MoveTimeOn + matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);
					string success;
					if (!result)
					{
						success = " " + Resources.wasNotSuccessful;
					}
					else
					{
						success = " " + Resources.wasSuccessful;
					}
					OnReportProgress(who + success + " " + newPeriodValue);
				}

			}
		}



		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				ResourceOptimizerProgressEventArgs args = new ResourceOptimizerProgressEventArgs(0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}
		
	}
}
