using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMoveTimeOptimizationExecuter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        bool Execute(IList<IScheduleDay> daysToDelete, IList<KeyValuePair<MoveTimeDay, IScheduleDay>> daysToSave, IList<IScheduleMatrixPro> allMatrixes, IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider);

        void Rollback(DateOnly dateOnly);
    }

    public enum MoveTimeDay
    {
        FirstDay,
        SecondDay
    };

    public class GroupMoveTimeOptimizationExecuter : IGroupMoveTimeOptimizationExecuter
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

        public GroupMoveTimeOptimizationExecuter(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IDeleteSchedulePartService deleteService, 
                                                 ISchedulingOptionsCreator schedulingOptionsCreator, IOptimizationPreferences optimizerPreferences, IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
                                                 IGroupMatrixHelper groupMatrixHelper, IGroupSchedulingService groupSchedulingService, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IResourceOptimizationHelper resourceOptimizationHelper)
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

        public bool Execute(IList<IScheduleDay> daysToDelete, IList<KeyValuePair<MoveTimeDay, IScheduleDay>> daysToSave, IList<IScheduleMatrixPro> allMatrixes, IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider)
        {
            _schedulePartModifyAndRollbackService.ClearModificationCollection();

            IList<IScheduleDay> cleanedList = new List<IScheduleDay>();
            if (daysToDelete != null)
                foreach (var scheduleDay in daysToDelete)
                {
                    SchedulePartView significant = scheduleDay.SignificantPart();
                    if (significant != SchedulePartView.FullDayAbsence && significant != SchedulePartView.DayOff && significant != SchedulePartView.ContractDayOff)
                        cleanedList.Add(scheduleDay);

                }
            _deleteService.Delete(cleanedList, _schedulePartModifyAndRollbackService);

            var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);

            if (daysToSave != null)
            {
                foreach(var pair in daysToSave  )
                {
                    if(pair.Key == MoveTimeDay.FirstDay )
                    {
                        schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;
                        if (!reSchedule(allMatrixes, optimizationOverLimitByRestrictionDecider, pair.Value , schedulingOptions))
                            return false;
                    }else if(pair.Key == MoveTimeDay.SecondDay  )
                    {
                        schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Short;
                        if (!reSchedule(allMatrixes, optimizationOverLimitByRestrictionDecider, pair.Value, schedulingOptions)) 
                            return false;
                    }
                }
            }

            return true;
        }

        private bool reSchedule(IList<IScheduleMatrixPro> allMatrixes,
                                IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider,
                                IScheduleDay scheduleDay, ISchedulingOptions schedulingOptions)
        {
            if (!scheduleDay.IsScheduled())
                return true;

            var significant = scheduleDay.SignificantPart();
            if (significant == SchedulePartView.FullDayAbsence || significant == SchedulePartView.DayOff ||
                significant == SchedulePartView.ContractDayOff)
                return true;

            var shiftDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
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

            var yes = optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit();
            if (yes)
            {
                Rollback(shiftDate);
                return false;
            }

            var result = optimizationOverLimitByRestrictionDecider.OverLimit();
            if (result.Count > 0)
            {
                Rollback(shiftDate);
                return false;
            }
            return true;
        }

        public void Rollback(DateOnly shiftDate)
        {
            _schedulePartModifyAndRollbackService.Rollback();
            _resourceOptimizationHelper.ResourceCalculateDate(shiftDate, true, true);
            _resourceOptimizationHelper.ResourceCalculateDate(shiftDate.AddDays(1), true, true);
        }
    }
}