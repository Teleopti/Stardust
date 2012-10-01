﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
    public interface IGroupSchedulingService
    {
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void Execute(DateOnlyPeriod selectedDays, IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, IList<IPerson> selectedPersons, BackgroundWorker backgroundWorker);
        bool ScheduleOneDay(DateOnly dateOnly, ISchedulingOptions schedulingOptions, IGroupPerson groupPerson, IList<IScheduleMatrixPro> matrixList);

    	bool ScheduleOneDayOnOnePerson(DateOnly dateOnly, IPerson person, ISchedulingOptions schedulingOptions,
    	                               IGroupPerson groupPerson, IList<IScheduleMatrixPro> matrixList);

        IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList, ISchedulingOptions schedulingOptions);
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
    	private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly IDeleteSchedulePartService _deleteSchedulePartService;
        private bool _cancelMe;
        
	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public GroupSchedulingService(IGroupPersonsBuilder groupPersonsBuilder,
            IBestBlockShiftCategoryFinder bestBlockShiftCategoryFinder, 
            ISchedulingResultStateHolder resultStateHolder, 
            IScheduleService scheduleService,
            ISchedulePartModifyAndRollbackService rollbackService,
			IResourceOptimizationHelper resourceOptimizationHelper,
            IWorkShiftFinderResultHolder finderResultHolder,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
            IDeleteSchedulePartService deleteSchedulePartService)
		{
			_groupPersonsBuilder = groupPersonsBuilder;
			_bestBlockShiftCategoryFinder = bestBlockShiftCategoryFinder;
		    _resultStateHolder = resultStateHolder;
		   _scheduleService = scheduleService;
			_rollbackService = rollbackService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_finderResultHolder = finderResultHolder;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		    _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		    _deleteSchedulePartService = deleteSchedulePartService;
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
                var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, selectedPersons, true, schedulingOptions);
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
			var agentAverageFairness = scheduleDictionary.AverageFairnessPoints(members);
			var best = groupPerson.CommonPossibleStartEndCategory;
			if(best == null)
			{
				IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { dateOnly }, new Dictionary<string, IWorkShiftFinderResult>());

				var bestCategoryResult = _bestBlockShiftCategoryFinder.BestShiftCategoryForDays(result, groupPerson, schedulingOptions, agentAverageFairness);
				best = bestCategoryResult.BestPossible;

				if (best == null && bestCategoryResult.FailureCause == FailureCause.NoValidPeriod)
					_finderResultHolder.AddFilterToResult(groupPerson, dateOnly, UserTexts.Resources.ErrorMessageNotAValidSchedulePeriod);

				if (best == null && bestCategoryResult.FailureCause == FailureCause.ConflictingRestrictions)
					_finderResultHolder.AddFilterToResult(groupPerson, dateOnly, UserTexts.Resources.ConflictingRestrictions);
			}


			if (best == null)
            {
                return false;
            }
			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(members, dateOnly, schedulingOptions, scheduleDictionary);
			foreach (var person in members.GetRandom(members.Count, true))
			{
				IScheduleDay scheduleDay = scheduleDictionary[person].ScheduledDay(dateOnly);

				if (scheduleDay.IsScheduled())
					continue;

				bool locked = false;
				foreach (var scheduleMatrixPro in matrixList)
				{
					if (scheduleMatrixPro.Person == scheduleDay.Person)
					{
						if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
						{
							if (!scheduleMatrixPro.UnlockedDays.Contains(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)))
							{
								locked = true;
							}
						}
					}
				}
				if (locked)
				{
					continue;
				}

				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
				                                                            schedulingOptions.ConsiderShortBreaks);

				bool sucess = _scheduleService.SchedulePersonOnDay(scheduleDay, schedulingOptions, true, effectiveRestriction,
				                                                   resourceCalculateDelayer, best,groupPerson );
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
			return true;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public bool ScheduleOneDayOnOnePerson(DateOnly dateOnly, IPerson person, ISchedulingOptions schedulingOptions, IGroupPerson groupPerson, IList<IScheduleMatrixPro> matrixList)
    	{
			var scheduleDictionary = _resultStateHolder.Schedules;

			if (groupPerson == null)
				return false;
			var members = groupPerson.GroupMembers;
			var agentAverageFairness = scheduleDictionary.AverageFairnessPoints(members);
			var best = groupPerson.CommonPossibleStartEndCategory;
			if (best == null)
			{
				IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { dateOnly }, new Dictionary<string, IWorkShiftFinderResult>());
			    var matrix = matrixList.First(d => d.Person == person);
                _workShiftMinMaxCalculator.ResetCache();
                var minmax = _workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, matrix, schedulingOptions);
                var bestCategoryResult = _bestBlockShiftCategoryFinder.BestShiftCategoryForDays(result, groupPerson, schedulingOptions, agentAverageFairness, minmax);
				best = bestCategoryResult.BestPossible;

				if (best == null && bestCategoryResult.FailureCause == FailureCause.NoValidPeriod)
					_finderResultHolder.AddFilterToResult(groupPerson, dateOnly, UserTexts.Resources.ErrorMessageNotAValidSchedulePeriod);

				if (best == null && bestCategoryResult.FailureCause == FailureCause.ConflictingRestrictions)
					_finderResultHolder.AddFilterToResult(groupPerson, dateOnly, UserTexts.Resources.ConflictingRestrictions);
			}


			if (best == null)
			{
				return false;
			}
			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(members, dateOnly, schedulingOptions, scheduleDictionary);

			IScheduleDay scheduleDay = scheduleDictionary[person].ScheduledDay(dateOnly);

			if (scheduleDay.IsScheduled())
				return true;

			bool locked = false;
			foreach (var scheduleMatrixPro in matrixList)
			{
				if (scheduleMatrixPro.Person == scheduleDay.Person)
				{
					if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
					{
						if (!scheduleMatrixPro.UnlockedDays.Contains(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)))
						{
							locked = true;
						}
					}
				}
			}
			if (locked)
			{
				return true;
			}

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																		schedulingOptions.ConsiderShortBreaks);

			bool sucess = _scheduleService.SchedulePersonOnDay(scheduleDay, schedulingOptions, true, effectiveRestriction,
															   resourceCalculateDelayer, best);
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

    		return true;
    	}

        public IList<IScheduleDay> DeleteMainShift(IList<IScheduleDay> schedulePartList, ISchedulingOptions schedulingOptions)
        {
            //Delete old current shift
            //TODO use a new Delete method with a rollbackservice
            var options = new DeleteOption { MainShift = true };

            IList<IScheduleDay> retList;
            using (var bgWorker = new BackgroundWorker())
            {
                retList = _deleteSchedulePartService.Delete(schedulePartList, options, _rollbackService, bgWorker);
            }

            //recalc resources
            ICollection<DateOnly> daysToRecalculate = new HashSet<DateOnly>();
            foreach (var part in schedulePartList)
            {
                var date = new DateOnly(part.Period.LocalStartDateTime);
                daysToRecalculate.Add(date);
                daysToRecalculate.Add(date.AddDays(1));
            }

            foreach (var date in daysToRecalculate)
            {
                _resourceOptimizationHelper.ResourceCalculateDate(date, true, schedulingOptions.ConsiderShortBreaks);
            }

            return retList;
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
