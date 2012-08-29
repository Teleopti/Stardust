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
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IDeleteSchedulePartService _deleteService;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly IOptimizationPreferences _optimizerPreferences;
		private readonly IGroupMatrixHelper _groupMatrixHelper;
		private readonly IGroupSchedulingService _groupSchedulingService;
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

		public GroupIntradayOptimizerService(IList<IGroupIntradayOptimizer> optimizers,
		                                     IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup,
		                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                     IDeleteSchedulePartService deleteService,
		                                     ISchedulingOptionsCreator schedulingOptionsCreator,
		                                     IMainShiftOptimizeActivitySpecificationSetter
		                                     	mainShiftOptimizeActivitySpecificationSetter,
		                                     IOptimizationPreferences optimizerPreferences,
		                                     IGroupMatrixHelper groupMatrixHelper,
		                                     IGroupSchedulingService groupSchedulingService,
		                                     IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			_optimizers = optimizers;
			_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_deleteService = deleteService;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_optimizerPreferences = optimizerPreferences;
			_groupMatrixHelper = groupMatrixHelper;
			_groupSchedulingService = groupSchedulingService;
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
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

		private IList<IGroupIntradayOptimizer> runTheList(IList<IGroupIntradayOptimizer> runningList, IList<IScheduleMatrixPro> allMatrixes)
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

				_schedulePartModifyAndRollbackService.ClearModificationCollection();
				_deleteService.Delete(daysToDelete, _schedulePartModifyAndRollbackService);

				var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
				foreach (var scheduleDay in daysToSave)
				{
					IPersonAssignment assignment = scheduleDay.AssignmentHighZOrder();
					if (assignment == null)
						continue;
					if (assignment.MainShift == null)
						continue;

					_mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences,
					                                                               assignment.MainShift,
					                                                               scheduleDay.DateOnlyAsPeriod.DateOnly);
					var success = _groupMatrixHelper.ScheduleSinglePerson(scheduleDay.DateOnlyAsPeriod.DateOnly, scheduleDay.Person,
					                                                      _groupSchedulingService,
					                                                      _schedulePartModifyAndRollbackService,
					                                                      schedulingOptions, _groupPersonBuilderForOptimization, allMatrixes);
					if (!success)
					{
						removeList.AddRange(memberList);
					}
				}
			}
			return removeList;
			//Create a class GroupIntraDayOptimizer that returns if succes it should use existing

			//Randomly select an lockablematrixconverter
			//On the first member use IntraDayDecisionMaker to find the day
			//Find the gropmembers for that day
			//save current schedules for the groupmembers on this day
			//clear rollbackService
			//delete schedules for all members this day
			//use groupMatrixHelper to rechedule the group
			//if sucess then add the groups lockablematrixconverters to sucess list
		}
	}
}