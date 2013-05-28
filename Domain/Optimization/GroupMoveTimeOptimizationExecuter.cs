using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMoveTimeOptimizationExecuter
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool Execute(IList<IScheduleDay> daysToDelete, IList<KeyValuePair<DayReadyToMove, IScheduleDay>> daysToSave,
			IList<IScheduleMatrixPro> allMatrixes, IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, ITeamSteadyStateHolder teamSteadyStateHolder, IScheduleDictionary scheduleDictionary);
        //void Rollback(DateOnly dateOnly);
        ISchedulingOptions SchedulingOptions { get; }
    	void Rollback(IScheduleDictionary scheduleDictionary);
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
    	private readonly IGroupMoveTimeOptimizationResourceHelper _groupMoveTimeOptimizationResourceHelper;
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
			IGroupMoveTimeOptimizationResourceHelper groupMoveTimeOptimizationResourceHelper)
			
        {
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _deleteService = deleteService;
            _schedulingOptionsCreator = schedulingOptionsCreator;
            _optimizerPreferences = optimizerPreferences;
            _mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
            _groupMatrixHelper = groupMatrixHelper;
            _groupSchedulingService = groupSchedulingService;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
        	_groupMoveTimeOptimizationResourceHelper = groupMoveTimeOptimizationResourceHelper;
        	_isSignificantPartFullDayAbsenceOrDayOffSpecification = new IsSignificantPartFullDayAbsenceOrDayOffSpecification();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public bool Execute(IList<IScheduleDay> daysToDelete, IList<KeyValuePair<DayReadyToMove, IScheduleDay>> daysToSave,
			IList<IScheduleMatrixPro> allMatrixes, IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, ITeamSteadyStateHolder teamSteadyStateHolder, IScheduleDictionary scheduleDictionary)
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
                //cleanedList.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct().ForEach(recalculateDay);
				_groupMoveTimeOptimizationResourceHelper.CalculateDeletedDays(cleanedList);
            }

            if (daysToSave != null)
            {
            	var scheduledFirstDate = false;
            	var scheduledSecondDate = false;
            	var teamSteadyStateSuccess = false;

            	foreach (var keyValuePair in daysToSave)
            	{
					if (!scheduledFirstDate && keyValuePair.Key.Equals(DayReadyToMove.FirstDay))
					{
						scheduledFirstDate = true;
						teamSteadyStateSuccess = reScheduleTeamSteadyState(allMatrixes, optimizationOverLimitByRestrictionDecider, keyValuePair, teamSteadyStateMainShiftScheduler, teamSteadyStateHolder, scheduleDictionary);
						if (!teamSteadyStateSuccess) 
							break;
					}	
						
					if (!scheduledSecondDate && keyValuePair.Key.Equals(DayReadyToMove.SecondDay))
					{
						scheduledSecondDate = true;
						teamSteadyStateSuccess = reScheduleTeamSteadyState(allMatrixes, optimizationOverLimitByRestrictionDecider, keyValuePair, teamSteadyStateMainShiftScheduler, teamSteadyStateHolder, scheduleDictionary);
						if (!teamSteadyStateSuccess) 
							break;
					}

					if (scheduledFirstDate && scheduledSecondDate)
					{
						break;
					}
            	}

				if (!teamSteadyStateSuccess)
				{
					foreach (var pair in daysToSave)
					{
						switch (pair.Key)
						{
							case DayReadyToMove.FirstDay:
								SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;	
								if (!reSchedule(allMatrixes, optimizationOverLimitByRestrictionDecider, pair.Value, SchedulingOptions, scheduleDictionary))
									return false;
								break;
							case DayReadyToMove.SecondDay:
								SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Short;
								if (!reSchedule(allMatrixes, optimizationOverLimitByRestrictionDecider, pair.Value, SchedulingOptions, scheduleDictionary))
									return false;
								break;
						}
					}
				}
            }
            return true;
        }

		private bool reScheduleTeamSteadyState(IList<IScheduleMatrixPro> allMatrixes, IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider, KeyValuePair<DayReadyToMove, IScheduleDay> keyValuePair, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, ITeamSteadyStateHolder teamSteadyStateHolder, IScheduleDictionary scheduleDictionary)
		{
			if (keyValuePair.Key.Equals(DayReadyToMove.FirstDay))
				SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;

			if (keyValuePair.Key.Equals(DayReadyToMove.SecondDay))
				SchedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Short;

			var dateOnly = keyValuePair.Value.DateOnlyAsPeriod.DateOnly;
			var groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(keyValuePair.Value.Person, dateOnly);

			if(teamSteadyStateHolder.IsSteadyState(groupPerson))
			{
				var teamSteadyStateSuccess = teamSteadyStateMainShiftScheduler.ScheduleTeam(dateOnly, groupPerson, _groupSchedulingService, _schedulePartModifyAndRollbackService, SchedulingOptions, _groupPersonBuilderForOptimization, allMatrixes, scheduleDictionary);
				if (teamSteadyStateSuccess)
				{
					if (optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit())
					{
						Rollback(scheduleDictionary);
						//Rollback(dateOnly);
						return false;
					}

					if (optimizationOverLimitByRestrictionDecider.OverLimit().Count > 0)
					{
						Rollback(scheduleDictionary);
						//Rollback(dateOnly);
						return false;
					}
				}
				else
				{
					return false;
				}	
			}

			else
			{
				return false;
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
                                IScheduleDay scheduleDay, ISchedulingOptions schedulingOptions, IScheduleDictionary scheduleDictionary)
        {
            if (!scheduleDay.IsScheduled())
                return true;

            var significant = scheduleDay.SignificantPart();
            if (_isSignificantPartFullDayAbsenceOrDayOffSpecification.IsSatisfiedBy(significant))
                return true;

            var shiftDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
            _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences,
                                                                           scheduleDay.GetEditorShift(),
                                                                           shiftDate);

            if (!_groupMatrixHelper.ScheduleSinglePerson(shiftDate, scheduleDay.Person,
                                                                  _groupSchedulingService,
                                                                  _schedulePartModifyAndRollbackService,
                                                                  schedulingOptions, _groupPersonBuilderForOptimization,
                                                                  allMatrixes))
                return false;

            if (optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit())
            {
				Rollback(scheduleDictionary);
                //Rollback(shiftDate);
                return false;
            }
            if (optimizationOverLimitByRestrictionDecider.OverLimit().Count > 0)
            {
				Rollback(scheduleDictionary);
                //Rollback(shiftDate);
                return false;
            }
            return true;
        }

		public void Rollback(IScheduleDictionary scheduleDictionary)
		{
			_groupMoveTimeOptimizationResourceHelper.Rollback(_schedulePartModifyAndRollbackService, scheduleDictionary);

			//var dates = new List<DateOnly>();
			//var modifiedDays = new List<IScheduleDay>();
			//var orgDays = new List<IScheduleDay>();

			//foreach (var modifiedSchedule in _schedulePartModifyAndRollbackService.ModificationCollection.ToList())
			//{
			//    if(!dates.Contains(modifiedSchedule.DateOnlyAsPeriod.DateOnly))
			//        dates.Add(modifiedSchedule.DateOnlyAsPeriod.DateOnly);
				
			//    modifiedDays.Add(modifiedSchedule);

			//    var scheduleRange = scheduleDictionary[modifiedSchedule.Person];
			//    var orgDay = scheduleRange.ScheduledDay(modifiedSchedule.DateOnlyAsPeriod.DateOnly);
			//    orgDays.Add(orgDay);
			//}

			//_schedulePartModifyAndRollbackService.Rollback();

			//foreach (var date in dates)
			//{
			//    var toRemove = new List<IScheduleDay>();
			//    var toAdd = new List<IScheduleDay>();

			//    var toRemoveNextDay = new List<IScheduleDay>();
			//    var toAddNextDay = new List<IScheduleDay>();

			//    foreach (var modifiedDay in modifiedDays)
			//    {
			//        if(modifiedDay.DateOnlyAsPeriod.DateOnly == date && modifiedDay.HasProjection)
			//        {
			//            toAdd.Add(modifiedDay);

			//            if(modifiedDay.Period.StartDateTime.Date != modifiedDay.Period.EndDateTime.Date)
			//                toAddNextDay.Add(modifiedDay);
			//        }
			//    }

			//    foreach (var orgDay in orgDays)
			//    {
			//        if(orgDay.DateOnlyAsPeriod.DateOnly == date && orgDay.HasProjection)
			//        {
			//            toRemove.Add(orgDay);

			//            if (orgDay.Period.StartDateTime.Date != orgDay.Period.EndDateTime.Date)
			//                toRemoveNextDay.Add(orgDay);
			//        }
			//    }

			//    if(toRemove.Count > 0 || toAdd.Count > 0)
			//    {
			//        _resourceOptimizationHelper.ResourceCalculateDate(date,true,true,toRemove,toAdd);
			//    }

			//    if(toRemoveNextDay.Count > 0 || toAddNextDay.Count > 0)
			//    {
			//        _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true, toRemoveNextDay, toAddNextDay);
			//    }
			//}
		}

		//public void Rollback(DateOnly dateOnly)
		//{
		//    _schedulePartModifyAndRollbackService.Rollback();
		//    recalculateDay(dateOnly);
		//}

		//private void recalculateDay(DateOnly dateOnly)
		//{
		//    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
		//    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly.AddDays(1), true, true);
		//}
    }
}