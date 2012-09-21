using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMoveTimeOptimizationExecuter
    {
        bool Execute(IList<IScheduleDay> daysToDelete, IList<KeyValuePair<DayReadyToMove, IScheduleDay>> daysToSave,
            IList<IScheduleMatrixPro> allMatrixes, IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider);
        void Rollback(DateOnly dateOnly);
        ISchedulingOptions SchedulingOptions { get; }
    }

    public enum DayReadyToMove
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
        private readonly IsSignificantPartFullDayAbsenceOrDayOffSpecification _isSignificantPartFullDayAbsenceOrDayOffSpecification;
        private ISchedulingOptions _schedulingOptions;

        public GroupMoveTimeOptimizationExecuter(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
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
            _isSignificantPartFullDayAbsenceOrDayOffSpecification = new IsSignificantPartFullDayAbsenceOrDayOffSpecification();
        }

        public bool Execute(IList<IScheduleDay> daysToDelete, IList<KeyValuePair<DayReadyToMove, IScheduleDay>> daysToSave,
            IList<IScheduleMatrixPro> allMatrixes, IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider)
        {
            _schedulePartModifyAndRollbackService.ClearModificationCollection();
            
            if (daysToDelete != null)
            {
                var cleanedList = (from scheduleDay in daysToDelete
                                   let significant = scheduleDay.SignificantPart()
                                   where
                                       !_isSignificantPartFullDayAbsenceOrDayOffSpecification.IsSatisfiedBy(significant)
                                   select scheduleDay).ToList();
                _deleteService.Delete(cleanedList, _schedulePartModifyAndRollbackService);
            }
            if (daysToSave != null)
            {
                foreach(var pair in daysToSave)
                {
                    switch (pair.Key)
                    {
                        case DayReadyToMove.FirstDay:
                            SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;
                            if (!reSchedule(allMatrixes, optimizationOverLimitByRestrictionDecider, pair.Value, SchedulingOptions))
                                return false;
                            break;
                        case DayReadyToMove.SecondDay:
                            SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Short;
                            if (!reSchedule(allMatrixes, optimizationOverLimitByRestrictionDecider, pair.Value, SchedulingOptions)) 
                                return false;
                            break;
                    }
                }
            }
            return true;
        }

        public ISchedulingOptions SchedulingOptions
        {
            get {
                return _schedulingOptions ??
                       (_schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences));
            }
        }

        private bool reSchedule(IList<IScheduleMatrixPro> allMatrixes,
                                IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider,
                                IScheduleDay scheduleDay, ISchedulingOptions schedulingOptions)
        {
            if (!scheduleDay.IsScheduled())
                return true;

            var significant = scheduleDay.SignificantPart();
            if (_isSignificantPartFullDayAbsenceOrDayOffSpecification.IsSatisfiedBy(significant))
                return true;

            var shiftDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
            _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences,
                                                                           scheduleDay.AssignmentHighZOrder().MainShift,
                                                                           shiftDate);

            if (!_groupMatrixHelper.ScheduleSinglePerson(shiftDate, scheduleDay.Person,
                                                                  _groupSchedulingService,
                                                                  _schedulePartModifyAndRollbackService,
                                                                  schedulingOptions, _groupPersonBuilderForOptimization,
                                                                  allMatrixes))
                return false;

            if (optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit())
            {
                Rollback(shiftDate);
                return false;
            }
            if (optimizationOverLimitByRestrictionDecider.OverLimit().Count > 0)
            {
                Rollback(shiftDate);
                return false;
            }
            return true;
        }

        public void Rollback(DateOnly dateOnly)
        {
            _schedulePartModifyAndRollbackService.Rollback();
            _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
            _resourceOptimizationHelper.ResourceCalculateDate(dateOnly.AddDays(1), true, true);
        }
    }
}