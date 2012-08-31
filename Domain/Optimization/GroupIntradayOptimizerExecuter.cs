using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupIntradayOptimizerExecuter
	{
		bool Execute(IList<IScheduleDay> daysToDelete, IList<IScheduleDay> daysToSave,
		             IList<IScheduleMatrixPro> allMatrixes,
		             IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider);
	}

	public class GroupIntradayOptimizerExecuter : IGroupIntradayOptimizerExecuter
	{
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IDeleteSchedulePartService _deleteService;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IOptimizationPreferences _optimizerPreferences;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly IGroupMatrixHelper _groupMatrixHelper;
		private readonly IGroupSchedulingService _groupSchedulingService;
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public GroupIntradayOptimizerExecuter(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
			IDeleteSchedulePartService deleteService, 
			ISchedulingOptionsCreator schedulingOptionsCreator, 
			IOptimizationPreferences optimizerPreferences,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IGroupMatrixHelper groupMatrixHelper,
			IGroupSchedulingService groupSchedulingService,
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
			IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_deleteService = deleteService;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_optimizerPreferences = optimizerPreferences;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_groupMatrixHelper = groupMatrixHelper;
			_groupSchedulingService = groupSchedulingService;
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool Execute(IList<IScheduleDay> daysToDelete, IList<IScheduleDay> daysToSave, 
			IList<IScheduleMatrixPro> allMatrixes, 
			IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider)
		{
			_schedulePartModifyAndRollbackService.ClearModificationCollection();
			_deleteService.Delete(daysToDelete, _schedulePartModifyAndRollbackService);

			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
			foreach (var scheduleDay in daysToSave)
			{
				if(!scheduleDay.IsScheduled())
					continue;

				DateOnly shiftDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
				_mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences,
																			   scheduleDay.AssignmentHighZOrder().MainShift,
																			   shiftDate);

				var success = _groupMatrixHelper.ScheduleSinglePerson(shiftDate, scheduleDay.Person,
																	  _groupSchedulingService,
																	  _schedulePartModifyAndRollbackService,
																	  schedulingOptions, _groupPersonBuilderForOptimization,
																	  allMatrixes);
				if (!success)
					return false;

				bool yes = optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit();
				if (yes)
				{
					_schedulePartModifyAndRollbackService.Rollback();
					_resourceOptimizationHelper.ResourceCalculateDate(shiftDate, true, true);
					_resourceOptimizationHelper.ResourceCalculateDate(shiftDate.AddDays(1), true, true);
					return false;
				}

				IList<DateOnly> result = optimizationOverLimitByRestrictionDecider.OverLimit();
				if(result.Count > 0)
				{
					_schedulePartModifyAndRollbackService.Rollback();
					_resourceOptimizationHelper.ResourceCalculateDate(shiftDate, true, true);
					_resourceOptimizationHelper.ResourceCalculateDate(shiftDate.AddDays(1), true, true);
					return false;
				}

			}
			return true;
		}
	}
}