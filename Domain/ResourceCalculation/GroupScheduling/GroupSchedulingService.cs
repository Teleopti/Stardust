using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
    public interface IGroupSchedulingService
    {
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void Execute(DateOnlyPeriod selectedDays, IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, IList<IPerson> selectedPersons, BackgroundWorker backgroundWorker);
        bool ScheduleOneDay(DateOnly dateOnly, ISchedulingOptions schedulingOptions, IGroupPerson groupPerson, IList<IScheduleMatrixPro> matrixList);
    }

    public class GroupSchedulingService : IGroupSchedulingService
    {
		private readonly IGroupPersonsBuilder _groupPersonsBuilder;
		private readonly IBestBlockShiftCategoryFinder _bestBlockShiftCategoryFinder;
	    private readonly ISchedulingResultStateHolder _resultStateHolder;
	   
		private readonly IScheduleService _scheduleService;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IWorkShiftFinderResultHolder _finderResultHolder;
		private bool _cancelMe;
        
	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public GroupSchedulingService(IGroupPersonsBuilder groupPersonsBuilder, IBestBlockShiftCategoryFinder bestBlockShiftCategoryFinder, 
            ISchedulingResultStateHolder resultStateHolder, IScheduleService scheduleService, ISchedulePartModifyAndRollbackService rollbackService, 
            IResourceOptimizationHelper resourceOptimizationHelper, IWorkShiftFinderResultHolder finderResultHolder)
		{
			_groupPersonsBuilder = groupPersonsBuilder;
			_bestBlockShiftCategoryFinder = bestBlockShiftCategoryFinder;
		    _resultStateHolder = resultStateHolder;
		   _scheduleService = scheduleService;
			_rollbackService = rollbackService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_finderResultHolder = finderResultHolder;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public void Execute(DateOnlyPeriod selectedDays, IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, 
			IList<IPerson> selectedPersons, BackgroundWorker backgroundWorker)
		{
            if(matrixList == null) throw new ArgumentNullException("matrixList");
            if(backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");

            _cancelMe = false;
			foreach (var dateOnly in selectedDays.DayCollection())
			{
                var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, selectedPersons, true);
				foreach (var groupPerson in groupPersons.GetRandom(groupPersons.Count, true))
				{
                    if (backgroundWorker.CancellationPending)
                        return;
                    _rollbackService.ClearModificationCollection();
                    if (!ScheduleOneDay(dateOnly, schedulingOptions, groupPerson, matrixList))
				    {
                        // add some information probably already added when trying to schedule
                        //AddResult(groupPerson, dateOnly, "XXCan't Schedule Team");
                        _rollbackService.Rollback();
                        _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
                        continue;
				    }
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool ScheduleOneDay(DateOnly dateOnly, ISchedulingOptions schedulingOptions, IGroupPerson groupPerson, IList<IScheduleMatrixPro> matrixList)
        {
            if(matrixList == null) throw new ArgumentNullException("matrixList");

            var scheduleDictionary = _resultStateHolder.Schedules;
            
            if (groupPerson == null)
                return false;
            var members = groupPerson.GroupMembers;
           
            IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { dateOnly }, new Dictionary<string, IWorkShiftFinderResult>());
            
			var bestCategoryResult = _bestBlockShiftCategoryFinder.BestShiftCategoryForDays(result, groupPerson, schedulingOptions);
            var best = bestCategoryResult.BestPossible;

			if (best == null && bestCategoryResult.FailureCause == FailureCause.NoValidPeriod)
                _finderResultHolder.AddFilterToResult(groupPerson, dateOnly, UserTexts.Resources.ErrorMessageNotAValidSchedulePeriod);

			if (best == null && bestCategoryResult.FailureCause == FailureCause.ConflictingRestrictions)
                _finderResultHolder.AddFilterToResult(groupPerson, dateOnly, UserTexts.Resources.ConflictingRestrictions);

			if (best == null)
            {
                return false;
            }

            foreach (var person in members.GetRandom(members.Count, true))
            {
                IScheduleDay scheduleDay = scheduleDictionary[person].ScheduledDay(dateOnly);

                bool locked = true;
                foreach (var scheduleMatrixPro in matrixList)
                {
                    if (scheduleMatrixPro.Person == scheduleDay.Person)
                    {
                        foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
                        {
                            if(scheduleDayPro.Day == scheduleDay.DateOnlyAsPeriod.DateOnly)
                            {
                                locked = false;
                                break;
                            }
                        }

                        if(locked == false) break;
                    }
                }

                if (!locked && !scheduleDay.IsScheduled())
                {
					var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																		schedulingOptions.ConsiderShortBreaks);
					bool sucess = _scheduleService.SchedulePersonOnDay(scheduleDay, schedulingOptions, true, resourceCalculateDelayer, best);
                    if (!sucess)
                    {
                        return false;

                    }
                    OnDayScheduled(new SchedulingServiceBaseEventArgs(scheduleDay));
                    if (_cancelMe)
                    {
                        _rollbackService.Rollback();
                        _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
                        return false;
                    }
                }
            }
            return true;
        }

        protected void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
        {
            EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
            if (temp != null)
            {
                temp(this, scheduleServiceBaseEventArgs);
                if (scheduleServiceBaseEventArgs.Cancel)
                    _cancelMe = true;
            }
        }

    }
}
