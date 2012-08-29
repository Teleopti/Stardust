using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class GroupIntradayOptimizerService
	{
		private readonly IList<IGroupIntradayOptimizer> _optimizers;
		private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
		private readonly IGroupIntradayOptimizerExecuter _groupIntradayOptimizerExecuter;

		public GroupIntradayOptimizerService(IList<IGroupIntradayOptimizer> optimizers,
		                                     IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup,
		                                     IGroupIntradayOptimizerExecuter groupIntradayOptimizerExecuter)
		{
			_optimizers = optimizers;
			_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
			_groupIntradayOptimizerExecuter = groupIntradayOptimizerExecuter;
		}

		public void Execute(IList<IScheduleMatrixPro> allMatrixes)
		{
			IList<IGroupIntradayOptimizer> runningList = new List<IGroupIntradayOptimizer>(_optimizers);

			while (runningList.Count > 0)
			{
				IList<IGroupIntradayOptimizer> removeList = runTheList(runningList, allMatrixes);
				foreach (var groupIntradayOptimizer in removeList)
				{
					runningList.Remove(groupIntradayOptimizer);
				}
			}
		}

		private IList<IGroupIntradayOptimizer> runTheList(IList<IGroupIntradayOptimizer> runningList,
		                                                  IList<IScheduleMatrixPro> allMatrixes)
		{
			IList<IGroupIntradayOptimizer> skipList = new List<IGroupIntradayOptimizer>();
			List<IGroupIntradayOptimizer> removeList = new List<IGroupIntradayOptimizer>();
			foreach (var optimizer in runningList.GetRandom(runningList.Count, true))
			{
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
					_groupOptimizerFindMatrixesForGroup.Find(optimizer.Person, selectedDate.Value);
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

				var success = _groupIntradayOptimizerExecuter.Execute(daysToDelete, daysToSave, allMatrixes);

				if (!success)
				{
					removeList.AddRange(memberList);
				}
			}
			return removeList;
		}
	}
}