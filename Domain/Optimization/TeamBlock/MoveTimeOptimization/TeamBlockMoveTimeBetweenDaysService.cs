using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeBetweenDaysService
	{
		void Execute(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder);
		event EventHandler<ResourceOptimizerProgressEventArgs> PerformMoveTime;
	}

	public class TeamBlockMoveTimeBetweenDaysService : ITeamBlockMoveTimeBetweenDaysService
	{
		public event EventHandler<ResourceOptimizerProgressEventArgs> PerformMoveTime;
		private ITeamBlockMoveTimeOptimizer _teamBlockMoveTimeOptimizer;
		private bool _cancelMe;

		public TeamBlockMoveTimeBetweenDaysService(ITeamBlockMoveTimeOptimizer teamBlockMoveTimeOptimizer)
		{
			_teamBlockMoveTimeOptimizer = teamBlockMoveTimeOptimizer;
		}

		public void Execute(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder)
		{

			var activeMatrixes = new List<IScheduleMatrixPro>(matrixList);
			while (activeMatrixes.Count > 0)
			{
				if (_cancelMe)
					break;
				IEnumerable<IScheduleMatrixPro> shuffledMatrixes = matrixList.GetRandom(matrixList.Count, true);
				int executes = 0;
				double lastPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
				double newPeriodValue = lastPeriodValue;
				foreach (var matrix in shuffledMatrixes)
				{
					if (_cancelMe)
						break;
					executes++;
					bool result = _teamBlockMoveTimeOptimizer.OptimizeMatrix( optimizerPreferences, matrixList, rollbackService, periodValueCalculator, schedulingResultStateHolder, matrix);
					if (!result)
					{
						activeMatrixes.Remove(matrix);
					}
					else
					{
						newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
					}

					//string unlocked = " (" +
					//						(int)
					//						(optimizer.Matrix.UnlockedDays.Count/
					//						(double) optimizer.Matrix.EffectivePeriodDays.Count*100) + "%) ";
					//string who = Resources.OptimizingShiftLengths + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + unlocked + optimizer.ContainerOwner.Name.ToString(NameOrderOption.FirstNameLastName);
					//string success;
					//if (!result)
					//{
					//	success = " " + Resources.wasNotSuccessful;
					//}
					//else
					//{
					//	success = " " + Resources.wasSuccessful;
					//}
					//string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ")";
					//OnReportProgress(who + success + values);
				}
			}

		}

		protected virtual void OnDayScheduled(ResourceOptimizerProgressEventArgs resourceOptimizerProgressEventArgs)
		{
			EventHandler<ResourceOptimizerProgressEventArgs> temp = PerformMoveTime;
			if (temp != null)
			{
				temp(this, resourceOptimizerProgressEventArgs);
			}
			_cancelMe = resourceOptimizerProgressEventArgs.Cancel;
		}

		public void Execute()
		{
			var someCondition = true;
			while (someCondition)
			{
				var dateOnlyList = getCandidatesDatesToAnalyze();
				if (dateOnlyList.Count == 0) break;

				var candidatesTeamBlock = contractTeamBlockOnDates(dateOnlyList);

				deleteDaysAmongTeamBlockBasedOnOptions(candidatesTeamBlock);

				resceduleTeamBlock(candidatesTeamBlock);

				validateIfMoveTimeIsOk(candidatesTeamBlock);
			}
		}

		

		private void validateIfMoveTimeIsOk(IList<TeamBlockInfo> candidatesTeamBlock)
		{
			throw new NotImplementedException();
		}

		private void resceduleTeamBlock(IList<TeamBlockInfo> candidatesTeamBlock)
		{
			throw new NotImplementedException();
		}

		private void deleteDaysAmongTeamBlockBasedOnOptions(IList<TeamBlockInfo> candidatesTeamBlock)
		{
			throw new NotImplementedException();
		}

		private IList<TeamBlockInfo> contractTeamBlockOnDates(IList<DateOnly> dateOnlyList)
		{
			throw new NotImplementedException();
		}

		private IList<DateOnly> getCandidatesDatesToAnalyze()
		{
			throw new NotImplementedException();
		}
	}
}
