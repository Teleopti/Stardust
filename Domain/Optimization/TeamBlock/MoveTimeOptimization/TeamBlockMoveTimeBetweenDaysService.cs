using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeBetweenDaysService
	{
		void Execute(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> matrixesOnSelectedperiod);
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockMoveTimeBetweenDaysService : ITeamBlockMoveTimeBetweenDaysService
	{
		private readonly ITeamBlockMoveTimeOptimizer _teamBlockMoveTimeOptimizer;
		private bool _cancelMe;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public TeamBlockMoveTimeBetweenDaysService(ITeamBlockMoveTimeOptimizer teamBlockMoveTimeOptimizer)
		{
			_teamBlockMoveTimeOptimizer = teamBlockMoveTimeOptimizer;
		}

		public void Execute(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> matrixesOnSelectedperiod)
		{

			var activeMatrixes = new List<IScheduleMatrixPro>(matrixesOnSelectedperiod);
			while (activeMatrixes.Count > 0)
			{
				if (_cancelMe)
					break;
				IEnumerable<IScheduleMatrixPro> shuffledMatrixes = matrixesOnSelectedperiod.GetRandom(matrixesOnSelectedperiod.Count, true);
				int executes = 0;
				foreach (var matrix in shuffledMatrixes)
				{
					if (_cancelMe)
						break;
					if (!selectedPersons.Contains(matrix.Person))
					{
						activeMatrixes.Remove(matrix);
						continue;
					}
					executes++;
					bool result = _teamBlockMoveTimeOptimizer.OptimizeMatrix( optimizerPreferences, matrixList, rollbackService, periodValueCalculator, schedulingResultStateHolder, matrix);
					if (!result)
					{
						activeMatrixes.Remove(matrix);
					}
					
					string who = "Move time on .. "  + matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);
					string success;
					if (!result)
					{
						success = " " + Resources.wasNotSuccessful;
					}
					else
					{
						success = " " + Resources.wasSuccessful;
					}
					OnReportProgress(who + success );
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
