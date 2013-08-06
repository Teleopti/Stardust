using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupIntradayOptimizerService
	{
		void Execute(IList<IScheduleMatrixPro> allMatrixes);
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		void OnReportProgress(string message);
	}

	public class GroupIntradayOptimizerService : IGroupIntradayOptimizerService
	{
		private readonly IList<IGroupIntradayOptimizer> _optimizers;
		private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
		private readonly IGroupIntradayOptimizerExecuter _groupIntradayOptimizerExecuter;
		private bool _cancelMe;

		public GroupIntradayOptimizerService(IList<IGroupIntradayOptimizer> optimizers,
		                                     IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup,
		                                     IGroupIntradayOptimizerExecuter groupIntradayOptimizerExecuter)
		{
			_optimizers = optimizers;
			_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
			_groupIntradayOptimizerExecuter = groupIntradayOptimizerExecuter;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Execute(IList<IScheduleMatrixPro> allMatrixes)
		{
			IList<IGroupIntradayOptimizer> runningList = new List<IGroupIntradayOptimizer>(_optimizers);

			while (runningList.Count > 0)
			{
				if (_cancelMe)
					return;

				IList<IGroupIntradayOptimizer> removeList = runTheList(runningList, allMatrixes);
				foreach (var groupIntradayOptimizer in removeList)
				{
					runningList.Remove(groupIntradayOptimizer);
				}
			}
		}

		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}

		private IList<IGroupIntradayOptimizer> runTheList(IList<IGroupIntradayOptimizer> runningList,
		                                                  IList<IScheduleMatrixPro> allMatrixes)
		{
			List<IGroupIntradayOptimizer> skipList = new List<IGroupIntradayOptimizer>();
			List<IGroupIntradayOptimizer> removeList = new List<IGroupIntradayOptimizer>();
			int executes = 0;
			foreach (var optimizer in runningList.GetRandom(runningList.Count, true))
			{
				IPerson person = optimizer.Person;
				executes++;
				if(_cancelMe)
					break;

				if (skipList.Contains(optimizer))
					continue;

				IList<IGroupIntradayOptimizer> memberList = new List<IGroupIntradayOptimizer>();
				DateOnly? selectedDate = optimizer.Execute();

				if (!selectedDate.HasValue)
				{
					skipList.Add(optimizer);
					removeList.Add(optimizer);
					continue;
				}

				IList<IScheduleMatrixPro> matrixes =
					_groupOptimizerFindMatrixesForGroup.Find(person, selectedDate.Value);
				foreach (var matrix in matrixes)
				{
					foreach (var groupIntradayOptimizer in runningList)
					{
						if (groupIntradayOptimizer.IsMatrixForDateAndPerson(selectedDate.Value, matrix.Person))
							memberList.Add(groupIntradayOptimizer);
					}
				}

				IList<IScheduleDay> daysToSave = new List<IScheduleDay>();
				IList<IScheduleDay> daysToDelete = new List<IScheduleDay>();
				foreach (var groupIntradayOptimizer in memberList)
				{
					IScheduleDay scheduleDay = groupIntradayOptimizer.Matrix.GetScheduleDayByKey(selectedDate.Value).DaySchedulePart();
					daysToSave.Add((IScheduleDay) scheduleDay.Clone());
					daysToDelete.Add(scheduleDay);
					groupIntradayOptimizer.LockDate(selectedDate.Value);
				}

                var oldPeriodValue = optimizer.PeriodValue(selectedDate.Value);

				var success = _groupIntradayOptimizerExecuter.Execute(daysToDelete, daysToSave, allMatrixes, optimizer.OptimizationOverLimitByRestrictionDecider);

                if (!success)
                {
                    removeList.AddRange(memberList);
                }
                var newPeriodValue = optimizer.PeriodValue(selectedDate.Value);
                var isPeriodWorse = newPeriodValue > oldPeriodValue;
                if (isPeriodWorse)
                {
                    _groupIntradayOptimizerExecuter.Rollback(selectedDate.Value);
                    reportProgress(selectedDate.Value, !success, runningList.Count, executes, person);
                }
                else
                    reportProgress(selectedDate.Value, success, runningList.Count, executes, person);
                skipList.AddRange(memberList);
				reportProgress(selectedDate.Value, success, runningList.Count, executes, person);
			}
			return removeList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.GroupIntradayOptimizerService.OnReportProgress(System.String)")]
		private void reportProgress(DateOnly date, bool result, int activeOptimizers, int executes, IPerson owner)
		{
			string dateString = date.ToShortDateString(TeleoptiPrincipal.Current.Regional.Culture);
			string who = Resources.OptimizingIntraday + Resources.Colon + "(" + activeOptimizers + ")" + executes + " " + dateString + " " + owner.Name.ToString(NameOrderOption.FirstNameLastName);
			string success;
			if (!result)
			{
				success = " " + Resources.wasNotSuccessful;
			}
			else
			{
				success = " " + Resources.wasSuccessful;
			}

			
			//string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ")";
			OnReportProgress(who + success);
		}
	}
}