using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeBetweenDaysService
	{
		void Execute(IOptimizationPreferences optimizerPreferences, IEnumerable<IScheduleMatrixPro> matrixList,
			ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator,
			ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IPerson> selectedPersons,
			DateOnlyPeriod selectedPeriod, IResourceCalculateDelayer resourceCalculateDelayer);
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockMoveTimeBetweenDaysService : ITeamBlockMoveTimeBetweenDaysService
	{
		private readonly ITeamBlockMoveTimeOptimizer _teamBlockMoveTimeOptimizer;
		private readonly IConstructTeamBlock  _constructTeamBlock;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly IFilterForNoneLockedTeamBlocks _filterForNoneLockedTeamBlocks;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public TeamBlockMoveTimeBetweenDaysService(ITeamBlockMoveTimeOptimizer teamBlockMoveTimeOptimizer,
			IConstructTeamBlock constructTeamBlock, IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
			IFilterForNoneLockedTeamBlocks filterForNoneLockedTeamBlocks)
		{
			_teamBlockMoveTimeOptimizer = teamBlockMoveTimeOptimizer;
			_constructTeamBlock = constructTeamBlock;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_filterForNoneLockedTeamBlocks = filterForNoneLockedTeamBlocks;
		}

		public void Execute(IOptimizationPreferences optimizerPreferences, IEnumerable<IScheduleMatrixPro> matrixList,
			ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator,
			ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IPerson> selectedPersons,
			DateOnlyPeriod selectedPeriod, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			var blocksToWorkWith = _constructTeamBlock.Construct(matrixList, selectedPeriod, selectedPersons,
				optimizerPreferences.Extra.BlockFinder(),
				optimizerPreferences.Extra.TeamGroupPage);

			blocksToWorkWith = _filterForTeamBlockInSelection.Filter(blocksToWorkWith, selectedPersons, selectedPeriod);

			blocksToWorkWith = _filterForNoneLockedTeamBlocks.Filter(blocksToWorkWith);
			var teamsToWorkWith = blocksToWorkWith.Select(s => s.TeamInfo).ToList();
			var cancel = false;
			var activeTeams = new List<ITeamInfo>(teamsToWorkWith);
			while (activeTeams.Any())
			{
				var team = activeTeams.GetRandom(activeTeams.Count, true).FirstOrDefault();
				if (team == null) break;
				var selectedMatrixes = team.MatrixesForMemberAndPeriod(team.GroupMembers.First(), selectedPeriod).ToList();

				IEnumerable<IScheduleMatrixPro> shuffledMatrixes =selectedMatrixes.GetRandom(selectedMatrixes.Count, true);
				foreach (var matrix in shuffledMatrixes)
				{
					if (cancel) return;

					bool result = _teamBlockMoveTimeOptimizer.OptimizeTeam(optimizerPreferences, team, matrix, rollbackService,
						periodValueCalculator, schedulingResultStateHolder, resourceCalculateDelayer);
					if (!result)
					{
						activeTeams.Remove(team);
					}
					double newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
					
					string who = Resources.MoveTimeOn + "("+ activeTeams.Count + ")" + team.Name;
					string success = !result ? " " + Resources.wasNotSuccessful : " " + Resources.wasSuccessful;

					var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, who + success + " " + newPeriodValue, optimizerPreferences.Advanced.RefreshScreenInterval, ()=>cancel=true));
					if (cancel || progressResult.ShouldCancel) return;
				}

			}
		}

		private CancelSignal onReportProgress(ResourceOptimizerProgressEventArgs args)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				handler(this, args);
				if (args.Cancel)
					return new CancelSignal {ShouldCancel = true};
			}
			return new CancelSignal();
		}
	}
}
